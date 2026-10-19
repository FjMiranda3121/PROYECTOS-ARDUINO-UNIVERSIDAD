using Microsoft.AspNetCore.Identity;

namespace EcoRutas.Api.Models
{
    public class Usuario
    {
        // Primary Key (GUID único global)
        public Guid Id { get; set; }

        public required string Nombre { get; set; }
        public required string Email { get; set; }

        // Hash encriptado de la contraseña (nunca texto plano)
        public required string PasswordHash { get; set; }

        // Clave Foránea (FK hacia la tabla Roles)
        public required int RolId { get; set; }

        // Timestamp de creación en UTC
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public Rol? Rol { get; set; }
        public ICollection<Recoleccion> Recolecciones { get; set; } = new List<Recoleccion>();
    }

}

