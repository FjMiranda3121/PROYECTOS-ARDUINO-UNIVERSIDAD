using EcoRutas.Api.Data;
using EcoRutas.Api.DTOs;
using EcoRutas.Api.Models;
using EcoRutas.Api.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace EcoRutas.Api.Services
{
    public class CanecaService : ICanecaService
    {
        public readonly AppDbContext _context;

        public CanecaService(AppDbContext context)
        {
            _context = context;
        }

        // 1. OBTENER TODAS (RF-06: Filtra inactivas por defecto)
        public async Task<List<CanecaResponse>> ObtenerTodasAsync(bool soloActivas = true)
        {
            var query = _context.Canecas.AsQueryable();
            if (soloActivas)
            {
                query = query.Where(c => c.Activa);
            }

            return await query
                .Select(c => new CanecaResponse
                {
                    Id = c.Id,
                    Ubicacion = c.Ubicacion,
                    ProfundidadMaxCm = c.ProfundidadMaxCm,
                    DistanciaCriticaCm = c.DistanciaCriticaCm,
                    EstadoActual = c.EstadoActual,
                    Activa= c.Activa,
                    Latitud = c.Latitud,
                    Longitud = c.Longitud
                })
                .ToListAsync();
        }
        // 2. OBTENER POR ID
        public async Task<CanecaResponse?> ObtenerPorIdAsync(string id)
        {
            var caneca = await _context.Canecas.FindAsync(id);
            if (caneca == null) return null;

            return new CanecaResponse
            {
                Id = caneca.Id,
                Ubicacion = caneca.Ubicacion,
                ProfundidadMaxCm = caneca.ProfundidadMaxCm,
                DistanciaCriticaCm = caneca.DistanciaCriticaCm,
                EstadoActual = caneca.EstadoActual,
                Activa = caneca.Activa,
                Latitud = caneca.Latitud,
                Longitud = caneca.Longitud
            };
        }
        // 3. CREAR CANECA
        public async Task<CanecaResponse> CrearAsync(CanecaCreateDto dto)
        {
            // Validar que el ID no exista previamente en Neon
            bool existe = await _context.Canecas.AnyAsync(c=> c.Id == dto.Id);
            if (existe)
            {
                throw new InvalidOperationException($"La caneca con ID '{dto.Id}' ya existe en el sistema.");
            }
            var caneca = new Caneca
            {
                Id = dto.Id,
                Ubicacion = dto.Ubicacion,
                ProfundidadMaxCm = dto.ProfundidadMaxCm,
                DistanciaCriticaCm = dto.DistanciaCriticaCm,
                Latitud = dto.Latitud,
                Longitud = dto.Longitud,
                Activa = true,
                EstadoActual = "NORMAL" // Estado inicial por defecto
            };

            _context.Canecas.Add(caneca);
            await _context.SaveChangesAsync();
            return new CanecaResponse
            {
                Id = caneca.Id,
                Ubicacion = caneca.Ubicacion,
                ProfundidadMaxCm = caneca.ProfundidadMaxCm,
                DistanciaCriticaCm = caneca.DistanciaCriticaCm,
                EstadoActual = caneca.EstadoActual,
                Activa = caneca.Activa,
                Latitud = caneca.Latitud,
                Longitud = caneca.Longitud
            };
        }
        // 4. ACTUALIZAR CANECA
        public async Task<CanecaResponse?> ActualizarAsync(string id, CanecaUpdateDto dto)
        {
            var caneca = await _context.Canecas.FindAsync(id);
            if(caneca == null) return null;

            caneca.Ubicacion=dto.Ubicacion;
            caneca.ProfundidadMaxCm=dto.ProfundidadMaxCm;
            caneca.DistanciaCriticaCm=dto.DistanciaCriticaCm;
            caneca.Latitud=dto.Latitud;
            caneca.Longitud=dto.Longitud;
            caneca.Activa=dto.Activa;

            await _context.SaveChangesAsync();

            return new CanecaResponse
            {
                Id = caneca.Id,
                Ubicacion = caneca.Ubicacion,
                ProfundidadMaxCm = caneca.ProfundidadMaxCm,
                DistanciaCriticaCm = caneca.DistanciaCriticaCm,
                EstadoActual = caneca.EstadoActual,
                Activa = caneca.Activa,
                Latitud = caneca.Latitud,
                Longitud = caneca.Longitud
            };
        }

        // 5. ELIMINACIÓN HÍBRIDA (RN07: Soft Delete / Hard Delete)
        public async Task<bool> EliminarAsync(string id)
        {
            var caneca = await _context.Canecas.FindAsync(id);
            if (caneca == null) return false;

            // Verificar si la caneca posee historial en Mediciones o Recolecciones
            bool tieneMediciones = await _context.Mediciones.AnyAsync(m => m.CanecaId == id);
            bool tieneRecolecciones = await _context.Recolecciones.AnyAsync(r =>r.CanecaId == id);

            if (tieneMediciones || tieneRecolecciones) 
            {
                //SOFT DELETE: Posee historial, no se puede borrar de la BD. Cambiamos la bandera a false.
                caneca.Activa=false;
            }
            else
            {
                //HARD DELETE: No tiene historial asociado. Se remueve físicamente de PostgreSQL.
                _context.Canecas.Remove(caneca);
            }
            await _context.SaveChangesAsync();
            return true;
        }
        
    }
}
