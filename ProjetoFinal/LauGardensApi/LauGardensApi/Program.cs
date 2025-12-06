using LauGardensApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Caching.StackExchangeRedis;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true; // opcional, só para o JSON ficar bonito
    });

// Usa a tua classe AppDbContext (do ficheiro que mostraste)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection")
            ?? "server=localhost;port=3306;database=LausGarden;user=root;password=root",
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "server=localhost;port=3306;database=LausGarden;user=root;password=root"
        )
    ));


//Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    // Tenta ler do appsettings.json, se não conseguir, usa localhost:6379 (o teu Docker)
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "LauGardens_"; // Prefixo para não misturar chaves
});

//Configuração do POLLY (Resiliência para APIs Externas/Imposter)
// 

// Define o que fazer quando falha (Retry): Tenta 3 vezes com tempo crescente
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

// Define o disjuntor (Circuit Breaker): Se falhar 5 vezes, pára por 30 segundos
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

// Regista o Cliente HTTP que vai falar com o Imposter (Mountebank)
builder.Services.AddHttpClient("ImposterApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:4545"); // Porta definida no Docker
})
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreakerPolicy);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization(); // Importante se formos adicionar JWT depois

app.MapControllers();

app.Run();