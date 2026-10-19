namespace EcoRutas.Api.DTOs
{
    public class AuthResponse
    {
        public required string Token { get; set; }
        public required string Email { get; set; }
        public required string Nombre { get; set; }
        public required string Rol { get; set; }
        public required DateTime Expiracion { get; set; }
    }
}
