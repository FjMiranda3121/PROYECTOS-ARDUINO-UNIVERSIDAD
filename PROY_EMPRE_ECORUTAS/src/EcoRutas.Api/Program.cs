using EcoRutas.Api.Configurations;
using EcoRutas.Api.Data;
using EcoRutas.Api.Models;
using EcoRutas.Api.Services;
using EcoRutas.Api.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Base de datos PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Opciones de JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// 1. CONFIGURACIÓN DE CORS (POLÍTICA POR DEFECTO)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "https://ecorutas-frontend.onrender.com",
                "https://ecorutas-frontend.vercel.app", // Sin la barra diagonal '/' al final
                "http://localhost:5500",
                "http://127.0.0.1:5500"
              )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 2. Inyección de Dependencias
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<MedicionService>();
builder.Services.AddScoped<ICanecaService, CanecaService>();
builder.Services.AddScoped<IRecoleccionService, RecoleccionService>();

// 3. Configuración del Middleware de Autenticación JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? "ClaveSeguraEcoRutasDefault2026!")),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings?.Issuer ?? "EcoRutasApi",
        ValidateAudience = true,
        ValidAudience = jwtSettings?.Audience ?? "EcoRutasClient",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllers();

var app = builder.Build();

// 4. MIDDLEWARES DE LA APLICACIÓN
app.UseRouting();

// Activar CORS (debe ejecutarse estrictamente antes de Authentication/Authorization)
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();