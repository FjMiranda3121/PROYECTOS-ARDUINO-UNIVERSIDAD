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
        public string? EstadoActual { get; set; }

        // Bandera para eliminación lógica (Soft Delete - RN07)
        public bool Activa { get; set; } = true;

        // Coordenadas GPS para renderizado en mapa (Leaflet.js)
        public required double Latitud { get; set; }
        public required double Longitud { get; set; }

        // Propiedades de navegación inversa
        public ICollection<Medicion> Mediciones { get; set; } = new List<Medicion>();
        public ICollection<Recoleccion> Recolecciones { get; set; } = new List<Recoleccion>();
    }
}
