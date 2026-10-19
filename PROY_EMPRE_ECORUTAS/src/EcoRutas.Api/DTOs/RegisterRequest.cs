namespace EcoRutas.Api.DTOs
{
    public class RegisterRequest
    {
        public required string Nombre { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required int RolId { get; set; }
    }
}
