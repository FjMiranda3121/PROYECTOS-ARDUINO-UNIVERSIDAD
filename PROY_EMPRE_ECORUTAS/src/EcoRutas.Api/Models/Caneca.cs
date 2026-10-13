namespace EcoRutas.Api.Models
{
    public class Caneca
    {
        //El Id será "CAN-001", por lo que usamos string en lugar de Guid o int
        public required string Id { get; set; }
        public required string Ubicacion { get; set; }
        public required double ProfundidadMaxCm { get; set; }
        public required double DistanciaCriticaCm { get; set; }

        // Nullable (?) porque al registrar la caneca por primera vez, no tiene estado
        public string? EstadoActual {  get; set; }


        // Relación: Una caneca tiene un historial de MUCHAS mediciones
        public ICollection<Medicion> Mediciones { get; set; } = new List<Medicion>();
    }
}
