namespace EcoRutas.Api.DTOs
{
    public class RecoleccionResponse
    {
        public Guid Id { get; set; }
        public required string CanecaId { get; set; }
        public required Guid UsuarioId { get; set; }
        public required string NombreOperador { get; set; }
        public required DateTime FechaHora { get; set; }
        public string? Observaciones { get; set; }
    }
}
