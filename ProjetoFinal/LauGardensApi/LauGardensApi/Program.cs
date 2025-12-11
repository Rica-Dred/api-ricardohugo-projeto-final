using LauGardensApi;
using LauGardensApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Polly.Extensions.Http;
using Polly.Utilities;
using System;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(
    new WebApplicationOptions
    {
        WebRootPath = "Frontend",
        Args = args
    });

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
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
    // Tenta ler do appsettings.json, se não conseguir, usa localhost:6379 (Docker)
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "LauGardens_"; // Prefixo para não misturar chaves
});

//Polly
builder.Services.AddSingleton<IAsyncCacheProvider, PollyRedisAdapt>();

//Configuração do POLLY 
// Define o que fazer quando falha: Tenta 3 vezes 
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


// JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"];

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS - para que qualquer origem comunique com a API
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTudo",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection(); //Redireciona p/ HTTPS

var frontendPath = Path.Combine(builder.Environment.ContentRootPath, "Frontend");

// Serve o index.html por defeito
app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = new PhysicalFileProvider(frontendPath),
    RequestPath = ""
});

// Serve os ficheiros estáticos da pasta Frontend
app.UseStaticFiles( new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(frontendPath),
    RequestPath = ""
}
); 

app.UseAuthentication(); //Autenticação JWT
app.UseAuthorization(); //Autorização JWT 

// CORS - Ativar Politica criada acima
app.UseCors("PermitirTudo");

app.MapControllers();


app.Run();