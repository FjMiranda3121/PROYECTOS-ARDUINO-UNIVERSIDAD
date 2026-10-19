namespace EcoRutas.Api.Models
{
    public class Rol
    {
        // Primary Key (Entero autoincremental en PostgreSQL)
        public int Id { get; set; }

        // Nombre del rol ("ADMIN" o "OPERADOR")
        public required string Nombre { get; set; }

        // Propiedad de navegación inversa: Un rol puede tener múltiples usuarios asignados (Relación 1:N)
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
