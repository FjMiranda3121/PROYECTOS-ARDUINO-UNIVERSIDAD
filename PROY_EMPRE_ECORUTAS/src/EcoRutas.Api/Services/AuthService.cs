using BCrypt.Net;
using EcoRutas.Api.Configurations;
using EcoRutas.Api.Data;
using EcoRutas.Api.DTOs;
using EcoRutas.Api.Models;
using EcoRutas.Api.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcoRutas.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthService(AppDbContext context, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest dto)
        {
            // 1. Buscar usuario e incluir la tabla de Roles (JOIN)
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (usuario == null) return null;

            // 2. Verificar si la contraseña ingresada coincide con el hash encriptado
            bool esValida = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);
            if (!esValida) return null;

            // 3. Generar token JWT y construir la respuesta
            return ConstruirRespuestaAuth(usuario);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest dto)
        {
            // 1. Validar que el correo no esté registrado
            bool existeEmail = await _context.Usuarios
                .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (existeEmail)
                throw new InvalidOperationException("El correo electrónico ya se encuentra registrado.");

            // 2. Validar que el rol exista en la base de datos
            var rol = await _context.Roles.FindAsync(dto.RolId);
            if (rol == null)
                throw new KeyNotFoundException("El Rol especificado no existe.");

            // 3. Encriptar la contraseña con BCrypt
            string hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 4. Crear la nueva entidad de usuario
            var nuevoUsuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email.ToLower(),
                PasswordHash = hash,
                RolId = dto.RolId,
                Rol = rol
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return ConstruirRespuestaAuth(nuevoUsuario);
        }

        private AuthResponse ConstruirRespuestaAuth(Usuario usuario)
        {
            var expiracion = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);
            string token = GenerarJwtToken(usuario, expiracion);

            return new AuthResponse
            {
                Token = token,
                Email = usuario.Email,
                Nombre = usuario.Nombre,
                Rol = usuario.Rol?.Nombre ?? "OPERADOR",
                Expiracion = expiracion
            };
        }

        private string GenerarJwtToken(Usuario usuario, DateTime expiracion)
        {
            // 1. Definir los Claims (Información embebida dentro del Token)
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "OPERADOR")
        };

            // 2. Crear la clave simétrica a partir de la SecretKey configurada
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. Crear el Token descriptor
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiracion,
                signingCredentials: creds
            );

            // 4. Serializar el Token a cadena de texto (HEADER.PAYLOAD.SIGNATURE)
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
