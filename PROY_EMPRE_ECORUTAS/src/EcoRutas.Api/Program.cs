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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 1. Configurar Opciones Fuertemente Tipadas (JwtSettings)
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// 2. Registrar el Servicio de Autenticación en la Inyección de Dependencias
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<MedicionService>();
builder.Services.AddScoped<ICanecaService, CanecaService>();
builder.Services.AddScoped<IRecoleccionService, RecoleccionService>();

// 3. Configurar el Middleware de Autenticación con JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // En producción Render se encarga de HTTPS
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Expira de forma exacta sin desfasajes de tiempo
    };
});

builder.Services.AddControllers();




var app = builder.Build();



app.UseRouting();

// ORDEN CRÍTICO DE SECURITY MIDDLEWARES:
// 1. Quien eres (Autenticación)
app.UseAuthentication();

// 2. Qué puedes hacer (Autorización / Roles)
app.UseAuthorization();

app.MapControllers();

app.Run();