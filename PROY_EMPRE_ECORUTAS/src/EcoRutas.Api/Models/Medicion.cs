namespace EcoRutas.Api.Models
{
    public class Medicion
    {
        //EF core reconocerá "Id" como la Primary Key automáticamente
        public Guid Id { get; set; }

        public required string CanecaId { get; set; }
        public required double DistanciaCm { get; set; }
        public required int PorcentajeLlenado { get; set; }
        public required string EstadoClasificado { get; set; }
        public DateTime FechaLectura { get; set; }

        // Propiedad de navegación: Permite acceder a los datos de la caneca desde la medición
        public Caneca? Caneca { get; set; }
    }
}
