namespace EcoRutas.Api.DTOs
{
    public class CanecaResponse
    {
        public required string Id { get; set; }
        public required string Ubicacion { get; set; }
        public required double ProfundidadMaxCm { get; set; }
        public required double DistanciaCriticaCm { get; set; }
        public string? EstadoActual { get; set; }
        public bool Activa { get; set; }
        public required double Latitud { get; set; }
        public required double Longitud { get; set; }
    }
}
