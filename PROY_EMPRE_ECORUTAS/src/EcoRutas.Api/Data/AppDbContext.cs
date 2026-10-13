using EcoRutas.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoRutas.Api.Data;


public class AppDbContext : DbContext
{
    //El constructor recibe las opciones de configuración (como la cadena de conexión)
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    //Representan las tablas  fisicas en la base de datos
    public DbSet<Caneca> Canecas => Set<Caneca>();
    public DbSet<Medicion> Mediciones => Set<Medicion>();

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

        // Configuración de la tabla Mediciones
        modelBuilder.Entity<Medicion>(entitiy =>
        {
            entitiy.HasKey(e => e.Id);

            //Relación estricta 1 a N
            entitiy.HasOne(m => m.Caneca)
                   .WithMany(c => c.Mediciones)
                   .HasForeignKey(m => m.CanecaId)
                   //Evita que se elimine una caneca accidentalmente si tiene historial
                   .OnDelete(DeleteBehavior.Restrict);

        });
    }
}
