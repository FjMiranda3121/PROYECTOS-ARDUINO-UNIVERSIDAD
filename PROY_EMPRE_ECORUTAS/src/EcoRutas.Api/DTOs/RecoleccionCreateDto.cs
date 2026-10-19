namespace EcoRutas.Api.DTOs
{
    public class RecoleccionCreateDto
    {
        public required string CanecaId { get; set; }

        // Campo opcional para notas de campo (ej. "Bolsa rota", "Tapa averiada")
        public string? Observaciones { get; set; }
    }
}
