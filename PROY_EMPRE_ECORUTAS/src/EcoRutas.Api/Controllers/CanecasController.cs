using EcoRutas.Api.DTOs;
using EcoRutas.Api.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRutas.Api.Controllers
{
    [ApiController]
    [Route("api/v3/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class CanecasController : ControllerBase
    {
        private readonly ICanecaService _canecaService;

        public CanecasController(ICanecaService canecaService)
        {
            _canecaService = canecaService;
        }

        // GET: /api/v3/canecas?soloActivas=true (RF-06)
        [HttpGet]
        public async Task<IActionResult> ObtenerTodas([FromQuery] bool soloActivas = true)
        {
            var canecas = await _canecaService.ObtenerTodasAsync(soloActivas);
            return Ok(canecas);
        }

        // GET: /api/v3/canecas/CAN-001
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(string id)
        {
            var caneca = await _canecaService.ObtenerPorIdAsync(id);
            if (caneca == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Caneca no encontrada",
                    Detail = $"No existe una caneca registrada con el ID '{id}'.",
                    Status = 404
                });
            }

            return Ok(caneca);
        }
        // POST: /api/v3/canecas (RF-03)
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CanecaCreateDto dto)
        {
            try
            {
                var nuevaCaneca = await _canecaService.CrearAsync(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevaCaneca.Id }, nuevaCaneca);
            }
            catch (InvalidOperationException ex) // Ej: El ID ya existe
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Conflicto de Creación",
                    Detail = ex.Message,
                    Status = 409
                });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Error interno", statusCode: 500);
            }
        }

        // PUT: /api/v3/canecas/CAN-001 (RF-04)
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(string id, [FromBody] CanecaUpdateDto dto)
        {
            var canecaActualizada = await _canecaService.ActualizarAsync(id, dto);
            if (canecaActualizada == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Caneca no encontrada",
                    Detail = $"No se encontró la caneca con el ID '{id}' para actualizar.",
                    Status = 404
                });
            }

            return Ok(canecaActualizada);
        }

        // DELETE: /api/v3/canecas/CAN-001 (RF-05 / RN07: Borrado Híbrido)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(string id)
        {
            bool eliminado = await _canecaService.EliminarAsync(id);
            if (!eliminado)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Caneca no encontrada",
                    Detail = $"No se encontró la caneca con el ID '{id}' para eliminar.",
                    Status = 404
                });
            }

            // Retorna HTTP 204 No Content indicando que la operación fue exitosa
            return NoContent();
        }
    }
}
