using EcoRutas.Api.Data;
using EcoRutas.Api.Models;
using EcoRutas.Api.DTOs;
using Microsoft.EntityFrameworkCore;
namespace EcoRutas.Api.Services
{
    public class MedicionService
    {
        private readonly AppDbContext _context;
        public MedicionService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<MedicionResponse> ProcesarMedicionAsync(MedicionRequest request)
        {
            // RN01 - Validación Física
            if (request.DistanciaCm < 0)
                throw new ArgumentException("La distancia medida no puede ser negativa.");

            //RN05 - Integridad de Dominio
            var caneca = await _context.Canecas.
                        FirstOrDefaultAsync(c => c.Id == request.CanecaId);
            if (caneca == null)
                throw new KeyNotFoundException($"La caneca {request.CanecaId} no está registrada.");

            //RN02 Acotamiento Lógico (Clamping)
            double distaciaAcotada = request.DistanciaCm;
            if (distaciaAcotada > caneca.ProfundidadMaxCm) distaciaAcotada = caneca.ProfundidadMaxCm;
            if (distaciaAcotada < caneca.DistanciaCriticaCm) distaciaAcotada = caneca.DistanciaCriticaCm;

            //RN03 - Fórmula de Porcentaje Dinámico
            double rangoTotal = caneca.ProfundidadMaxCm - caneca.DistanciaCriticaCm;
            double llenado = caneca.ProfundidadMaxCm - distaciaAcotada;
            int porcentaje = (int)Math.Round((llenado / rangoTotal) * 100);

            //RN04 - Asignación de Estados
            string estado = "NORMAL";
            if (porcentaje >= 50 && porcentaje <= 84) estado = "ATENCION";
            else if (porcentaje >= 85) estado = "CRITICO";

            //Preparar la transacción (Historial transaccional)
            var nuevaMedicion = new Medicion
            {
                CanecaId = caneca.Id,
                DistanciaCm = request.DistanciaCm, // Guardamos la cruda para auditoria
                PorcentajeLlenado = porcentaje,
                EstadoClasificado = estado,
                FechaLectura = DateTime.UtcNow
            };

            //Desnomalización controlada: actualizamos el estado de la canena
            caneca.EstadoActual = estado;

            //Persitencia (RN06) - Entity Framework ejecuta ambos en una transacción
            _context.Mediciones.Add(nuevaMedicion);
            await _context.SaveChangesAsync();

            return new MedicionResponse(
                nuevaMedicion.Id,
                caneca.Id,
                porcentaje,
                estado, nuevaMedicion.FechaLectura
                );
        }

        public async Task<object> ConsultarHistorialAsync(string? canecaId, int page, int limit)
        {
            // 1. Preparamos la consulta base
            var query = _context.Mediciones.AsQueryable();

            // 2. Si nos piden una caneca específica, filtramos
            if (!string.IsNullOrWhiteSpace(canecaId))
            {
                query = query.Where(m => m.CanecaId == canecaId);
            }

            // 3. Contamos el total para que el Frontend sepa cuántas páginas hay
            int totalRegistros = await query.CountAsync();

            // 4. Aplicamos ordenamiento (las más nuevas primero) y paginación
            var datos = await query
                .OrderByDescending(m => m.FechaLectura)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(m => new MedicionResponse(
                    m.Id,
                    m.CanecaId,
                    m.PorcentajeLlenado,
                    m.EstadoClasificado,
                    m.FechaLectura))
                .ToListAsync();

            // Retornamos un objeto estructurado
            return new
            {
                Total = totalRegistros,
                PaginaActual = page,
                LimitePorPagina = limit,
                Datos = datos
            };
        }

    }
}