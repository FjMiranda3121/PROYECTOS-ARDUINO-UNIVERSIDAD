using EcoRutas.Api.DTOs;
using EcoRutas.Api.Services;
using EcoRutas.Api.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoRutas.Api.Controllers
{
    [ApiController]
    [Route("api/v3/[controller]")] // /api/v3/recolecciones
    [Authorize(Roles = "ADMIN,OPERADOR")] // Accesible para Administradores y Operadores de Aseo
    public class RecoleccionesController : ControllerBase
    {
        private readonly IRecoleccionService _recoleccionService;

        public RecoleccionesController(IRecoleccionService recoleccionService)
        {
            _recoleccionService = recoleccionService;
        }

        /// <summary>
        /// Retorna los puntos criticos y en atencion exclusivamente para la ruta del operador.
        /// </summary>
        [HttpGet("pendientes")]
        [Authorize(Roles = "OPERADOR,ADMIN")]
        public async Task<IActionResult> ObtenerRutasPendientes()
        {
            var rutas = await _recoleccionService.ObtenerRutasPendientesAsync();
            return Ok(rutas);
        }

        // POST: /api/v3/recolecciones (RF-08 & RN08: Confirmación de Vaciado)
        [HttpPost]
        public async Task<IActionResult> RegistrarRecoleccion([FromBody] RecoleccionCreateDto dto)
        {
            try
            {
                //  Extracción de seguridad: Obtenemos el UsuarioId directo del Claim del Token JWT
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var usuarioId))
                {
                    return Unauthorized(new ProblemDetails
                    {
                        Title = "Identidad no verificada",
                        Detail = "No se pudo validar el identificador del operador dentro del token JWT.",
                        Status = 401
                    });
                }

                var response = await _recoleccionService.RegistrarRecoleccionAsync(dto, usuarioId);
                return Created(string.Empty, response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Recurso no encontrado",
                    Detail = ex.Message,
                    Status = 404
                });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Error interno", statusCode: 500);
            }
        }

        // GET: /api/v3/recolecciones/ruta (RF-09: Vista de Ruta Operativa)
        [HttpGet("ruta")]
        public async Task<IActionResult> ObtenerRutaOperativa()
        {
            var ruta = await _recoleccionService.ObtenerRutaOperativaAsync();
            return Ok(ruta);
        }

        // GET: /api/v3/recolecciones/historial (Auditoría)
        [HttpGet("historial")]
        public async Task<IActionResult> ObtenerHistorial([FromQuery] string? canecaId = null)
        {
            var historial = await _recoleccionService.ObtenerHistorialAsync(canecaId);
            return Ok(historial);
        }
    }
}
