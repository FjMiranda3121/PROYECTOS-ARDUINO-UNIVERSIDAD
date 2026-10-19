using EcoRutas.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using EcoRutas.Api.Services.Interface;

namespace EcoRutas.Api.Controllers
{
    [ApiController]
    [Route("api/v3/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ENDPOINT 1: Registro de un nuevo usuario
        // POST: /api/v3/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest dto)
        {
            try
            {
                var response = await _authService.RegisterAsync(dto);

                //Retorna HTTP 201 Created junto con el Token JWT
                return Created(string.Empty, response);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Conflicto de Registro",
                    Detail = ex.Message,
                    Status = 409
                });
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

        //ENDPOINT 2: Inicio de sesión
        // POST: /api/v3/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest dto)
        {
            try
            {
                var response = await _authService.LoginAsync(dto);

                // Retorna HTTP 200 OK con los datos del usuario y el Token JWT
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Acceso Denegado",
                    Detail = ex.Message,
                    Status = 401
                });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Error interno", statusCode: 500);
            }

        }
    }

}
