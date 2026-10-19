namespace EcoRutas.Api.DTOs
{
    public class LoginRequest
    {
        // Datos en texto plano ingresados por el usuario humano
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
