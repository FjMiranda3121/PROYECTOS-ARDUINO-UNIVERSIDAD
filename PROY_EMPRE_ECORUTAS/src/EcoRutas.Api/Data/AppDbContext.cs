using EcoRutas.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoRutas.Api.Data;


public class AppDbContext : DbContext
{
    //El constructor recibe las opciones de configuración (como la cadena de conexión)
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    //Representan las tablas  fisicas en la base de datos (v2.0)
    public DbSet<Caneca> Canecas => Set<Caneca>();
    public DbSet<Medicion> Mediciones => Set<Medicion>();

    // NUEVAS ENTIDADES V3.0
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet <Usuario> Usuarios => Set<Usuario>();

    public DbSet <Recoleccion> Recolecciones => Set<Recoleccion>();

    //Aqui blidamos las reglas de negocio y relaciones
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Configuración de la tabla Canecas
        modelBuilder.Entity<Caneca>(entity =>
            {
                entity.HasKey(e => e.Id);

                //Evita que EF Core intente autogenerar el ID, ya que nosotros pasaremos "CAN-001"
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

        // 1. Unicidad
        modelBuilder.Entity<Rol>()
            .HasIndex(r => r.Nombre)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // 2. Relaciones y Comportamiento de Borrado Defensivo
        modelBuilder.Entity<Medicion>()
            .HasOne(m => m.Caneca)
            .WithMany(c => c.Mediciones)
            .HasForeignKey(m => m.CanecaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Recoleccion>()
            .HasOne(r => r.Caneca)
            .WithMany(c => c.Recolecciones)
            .HasForeignKey(r => r.CanecaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Recoleccion>()
            .HasOne(r => r.Usuario)
            .WithMany(u => u.Recolecciones)
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(u => u.RolId)
            .OnDelete(DeleteBehavior.Restrict);

        // 3. Data Seeding Inicial (Roles predeterminados)
        modelBuilder.Entity<Rol>().HasData(
            new Rol { Id = 1, Nombre = "ADMIN" },
            new Rol { Id = 2, Nombre = "OPERADOR" }
        );

        
    }
}
