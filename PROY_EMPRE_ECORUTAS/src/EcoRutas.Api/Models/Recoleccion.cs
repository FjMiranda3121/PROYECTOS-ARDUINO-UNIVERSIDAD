namespace EcoRutas.Api.Models
{
    public class Recoleccion
    {
        // Primary Key
        public Guid Id { get; set; }

        // Claves Foráneas
        public required string CanecaId { get; set; }
        public required Guid UsuarioId { get; set; }

        // Timestamp exacto del vaciado en UTC
        public DateTime FechaHora { get; set; } = DateTime.UtcNow;

        // Observaciones opcionales ingresadas por el aseador en campo
        public string? Observaciones { get; set; }

        // Propiedades de navegación
        public Caneca? Caneca { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
