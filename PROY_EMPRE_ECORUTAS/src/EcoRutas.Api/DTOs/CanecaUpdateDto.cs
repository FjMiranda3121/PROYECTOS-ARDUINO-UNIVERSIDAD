namespace EcoRutas.Api.DTOs
{
    public class CanecaUpdateDto
    {
        public required string Ubicacion { get; set; }
        public required double ProfundidadMaxCm { get; set; }
        public required double DistanciaCriticaCm { get; set; }
        public required double Latitud { get; set; }
        public required double Longitud { get; set; }
        public bool Activa { get; set; } = true;
    }
}
