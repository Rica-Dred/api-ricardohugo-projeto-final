using LauGardensApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();