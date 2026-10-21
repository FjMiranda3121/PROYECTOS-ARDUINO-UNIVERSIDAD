using EcoRutas.Api.Data;
using EcoRutas.Api.DTOs;
using EcoRutas.Api.Models;
using EcoRutas.Api.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace EcoRutas.Api.Services
{
    public class RecoleccionService : IRecoleccionService
    {
        private readonly AppDbContext _context;

        public RecoleccionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RutaPendienteDto>> ObtenerRutasPendientesAsync()
        {
            // 1. Consultar a la base de datos con EF Core
            var canecasPendientes = await _context.Canecas
                .Where(c => c.Activa && (c.EstadoActual == "CRITICO" || c.EstadoActual == "ATENCION"))
                .Select(c => new
                {
                    c.Id,
                    c.Ubicacion,
                    c.Latitud,
                    c.Longitud,
                    EstadoActual = c.EstadoActual ?? "DESCONOCIDO",
                    c.ProfundidadMaxCm,
                    // Proyección de la última distancia registrada en Mediciones
                    UltimaDistancia = c.Mediciones
                        .OrderByDescending(m => m.FechaLectura) // Ajustar al nombre del campo de fecha en tu entidad Medicion
                        .Select(m => (double?)m.DistanciaCm) // Ajustar al nombre de la propiedad de distancia en Medicion
                        .FirstOrDefault()
                })
                .ToListAsync();

            // 2. Calcular el porcentaje de llenado proyectando el DTO en memoria
            return canecasPendientes.Select(c =>
            {
                double porcentaje = 0;

                if (c.UltimaDistancia.HasValue && c.ProfundidadMaxCm > 0)
                {
                    porcentaje = ((c.ProfundidadMaxCm - c.UltimaDistancia.Value) / c.ProfundidadMaxCm) * 100;
                    porcentaje = Math.Clamp(Math.Round(porcentaje, 2), 0, 100);
                }

                return new RutaPendienteDto(
                    c.Id,
                    c.Ubicacion,
                    c.Latitud,
                    c.Longitud,
                    c.EstadoActual,
                    porcentaje
                );
            })
            .OrderByDescending(r => r.PorcentajeLlenado);
        }

        // 1. REGISTRAR RECOLECCIÓN (RF-08 & RN08) 
        public async Task<RecoleccionResponse> RegistrarRecoleccionAsync(RecoleccionCreateDto dto, Guid usuarioId)
        {
            // Validar existencia de la caneca y que esté activa (RN05)
            var caneca = await _context.Canecas.FirstOrDefaultAsync(c => c.Id == dto.CanecaId && c.Activa);
            if (caneca == null)
            {
                throw new KeyNotFoundException($"La caneca con ID '{dto.CanecaId}' no existe o se encuentra inactiva.");
            }

            // Validar que el usuario que ejecuta la acción exista en la base de datos
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
            {
                throw new KeyNotFoundException("El operador de campo no existe en el sistema.");
            }

            // Instanciar la entidad de auditoría
            var recoleccion = new Recoleccion
            {
                CanecaId = dto.CanecaId,
                UsuarioId = usuarioId,
                FechaHora = DateTime.UtcNow,
                Observaciones = dto.Observaciones
            };

            _context.Recolecciones.Add(recoleccion);

            // APLICACIÓN DE LA REGLA RN08:
            // Al confirmarse el vaciado físico, la caneca restablece su estado y su porcentaje a 0%
            caneca.EstadoActual = "NORMAL";
            caneca.PorcentajeLlenado = 0; // <--- CORRECCIÓN 1: Resetear porcentaje en la entidad

            // CORRECCIÓN 2: Insertar nueva medición de 0% en la tabla de telemetría para el Dashboard
            var medicionVaciado = new Medicion
            {
                Id = Guid.NewGuid(),
                CanecaId = dto.CanecaId,
                DistanciaCm = 100, // Distancia máxima equivalente a contenedor vacío
                PorcentajeLlenado = 0,
                EstadoClasificado = "NORMAL",
                FechaLectura = DateTime.UtcNow
            };
            _context.Mediciones.Add(medicionVaciado);

            // Persistencia transaccional atómica en PostgreSQL (RN06)
            await _context.SaveChangesAsync();

            return new RecoleccionResponse
            {
                Id = recoleccion.Id,
                CanecaId = recoleccion.CanecaId,
                UsuarioId = recoleccion.UsuarioId,
                NombreOperador = usuario.Nombre,
                FechaHora = recoleccion.FechaHora,
                Observaciones = recoleccion.Observaciones
            };
        }

        // 2. VISTA DE RUTA OPERATIVA (RF-09) 
        public async Task<List<RutaOperativaResponse>> ObtenerRutaOperativaAsync()
        {
            // Consultar únicamente canecas activas en estado CRITICO o ATENCION
            var canecasPrioritarias = await _context.Canecas
                .Where(c => c.Activa && (c.EstadoActual == "CRITICO" || c.EstadoActual == "ATENCION"))
                .ToListAsync();

            // Jerarquizar algorítmicamente por nivel de urgencia: 1 (CRITICO) > 2 (ATENCION)
            return canecasPrioritarias
                .Select(c => new RutaOperativaResponse
                {
                    CanecaId = c.Id,
                    Ubicacion = c.Ubicacion,
                    EstadoActual = c.EstadoActual ?? "NORMAL",
                    Latitud = c.Latitud,
                    Longitud = c.Longitud,
                    UrgenciaPrioridad = c.EstadoActual == "CRITICO" ? 1 : 2
                })
                .OrderBy(r => r.UrgenciaPrioridad)
                .ToList();
        }

        // 3. HISTORIAL DE AUDITORÍA 
        public async Task<List<RecoleccionResponse>> ObtenerHistorialAsync(string? canecaId = null)
        {
            var query = _context.Recolecciones
                .Include(r => r.Usuario)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(canecaId))
            {
                query = query.Where(r => r.CanecaId == canecaId);
            }

            return await query
                .OrderByDescending(r => r.FechaHora)
                .Select(r => new RecoleccionResponse
                {
                    Id = r.Id,
                    CanecaId = r.CanecaId,
                    UsuarioId = r.UsuarioId,
                    NombreOperador = r.Usuario!.Nombre,
                    FechaHora = r.FechaHora,
                    Observaciones = r.Observaciones
                })
                .ToListAsync();
        }
    }
}
