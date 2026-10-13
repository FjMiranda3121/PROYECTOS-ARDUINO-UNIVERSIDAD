using Microsoft.AspNetCore.Mvc;
using EcoRutas.Api.DTOs;
using EcoRutas.Api.Services;
using EcoRutas.Api.Security;
namespace EcoRutas.Api.Controllers
{

    [ApiController]
    [Route("api/v2/[controller]")]
    public class MedicionesController : ControllerBase
    {
        private readonly MedicionService _mediicionService;

        //Inyección de Dependecias: ASP .NET Core nos entrega el servicio automaticamente
        public MedicionesController(MedicionService mediicionService)
        {
            _mediicionService = mediicionService;
        }

        [HttpPost]
        [ApiKey]
        public async Task<IActionResult> RegistrarMedicion([FromBody] MedicionRequest request)
        {
            try
            {
                // Le pasamos el problema al "cerebro" (el Servicio)
                var response = await _mediicionService.ProcesarMedicionAsync(request);

                // Si todo sale bien, devolvemos un HTTP 201 Created con el JSON de respuesta
                return Created("", response);
            }
            catch (ArgumentException ex)
            {
                // Si el servicio detecta una distancia negativa (RN01)
                return BadRequest(new ProblemDetails
                {
                    Type = "https://ecorutas.com/errors/validation",
                    Title = "Error de validacion de datos",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            catch (KeyNotFoundException ex)
            {
                // Si el servicio no encuentra la caneca (RN05)
                return NotFound(new ProblemDetails
                {
                    Type = "https://ecorutas.com/errors/not-found",
                    Title = "Recurso no encontrado",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
            catch (Exception)
            {
                // Para cualquier otro error inesperado (Fallo en BD, etc.)
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails

                {
                    Type = "https://ecorutas.com/errors/internal",
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un problema inesperado procesando la medición",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerHistorial(
        [FromQuery] string? canecaId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 50)
        {
            // RT08 - Paginación Defensiva: Evitamos que pidan más de 100 de un golpe
            if (limit > 100) limit = 100;
            if (page < 1) page = 1;

            var historial = await _mediicionService.ConsultarHistorialAsync(canecaId, page, limit);

            return Ok(historial); // Devuelve HTTP 200 OK con los datos
        }
    }
}
