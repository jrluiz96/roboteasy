using Microsoft.EntityFrameworkCore;
using Api.Configuration;
using Api.Data;
using Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configurações
builder.Services.AddApplicationServices(builder.Configuration);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware de erro
app.UseErrorHandling();

// Swagger (apenas em desenvolvimento)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Mapeia controllers
app.MapControllers();

// Rota de health check
app.MapGet("/", () => "API is running!");

// Migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

