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
        options.JsonSerializerOptions.WriteIndented = true; // opcional, para o JSON ficar bonito
    });

//AppDbContext - config BD
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
    // Tenta ler do appsettings.json, se nao conseguir, usa localhost:6379 (Docker)
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "LauGardens_"; // Prefixo para n�o misturar chaves
});

//Polly
builder.Services.AddSingleton<IAsyncCacheProvider, PollyRedisAdapt>();

//Config. Polly 
//Tenta 3 vezes caso haja falha
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

//Se falhar 5 vezes, para por 30 segundos
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

//Regista o Cliente HTTP que vai falar com o Imposter 
builder.Services.AddHttpClient("ImposterApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:4545"); // Porta definida no Docker
})
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreakerPolicy);


//JWT
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
            .AllowAnyMethod() //qualquer método (GET, POST...)
            .AllowAnyHeader()); //qualquer cabecalho
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

// Serve os ficheiros est�ticos da pasta Frontend
app.UseStaticFiles( new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(frontendPath),
    RequestPath = ""
}
); 

app.UseAuthentication(); //Autenticacao JWT
app.UseAuthorization(); //Autorizacao JWT 

// CORS - Ativar Politica criada acima
app.UseCors("PermitirTudo");

app.MapControllers();


// Cria a BD se no existir (Substitui Migrations)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao criar a Base de Dados.");
    }
}

app.Run();