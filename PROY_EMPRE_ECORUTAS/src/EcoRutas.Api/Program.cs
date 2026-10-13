
using EcoRutas.Api.Data;
using EcoRutas.Api.Models;
using EcoRutas.Api.Services;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

// 1. Registrar base de datos y servicios
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<MedicionService>();
builder.Services.AddControllers();
// RT10 - Habilitar CORS (Permitir peticiones desde navegadores web)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// RT09 - Servicio de monitoreo (Healthcheck)
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 2. Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization(); // Importante para la seguridad futura
app.UseCors("AllowAll");
app.MapControllers();



// 3. Sembrado de datos limpio y directo
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    db.Database.EnsureCreated();

    if (!db.Canecas.Any(c => c.Id == "CAN-001"))
    {
        db.Canecas.Add(new Caneca
        {
            Id = "CAN-001",
            Ubicacion = "Laboratorio Prototipo",
            ProfundidadMaxCm = 30.0,
            DistanciaCriticaCm = 6.0,
            EstadoActual = "NORMAL"
        });

        db.SaveChanges();
    }
}
app.MapHealthChecks("/health");
app.Run();