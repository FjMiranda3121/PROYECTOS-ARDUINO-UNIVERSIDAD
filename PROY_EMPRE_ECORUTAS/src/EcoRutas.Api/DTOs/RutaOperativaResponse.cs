namespace EcoRutas.Api.DTOs
{
    public class RutaOperativaResponse
    {
        public required string CanecaId { get; set; }
        public required string Ubicacion { get; set; }
        public required string EstadoActual { get; set; }
        public required double Latitud { get; set; }
        public required double Longitud { get; set; }
        public required int UrgenciaPrioridad { get; set; } // 1: CRITICO, 2: ATENCION
    }
}
