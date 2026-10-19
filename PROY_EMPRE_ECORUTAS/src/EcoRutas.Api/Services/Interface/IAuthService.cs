using EcoRutas.Api.DTOs;
namespace EcoRutas.Api.Services.Interface
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest dto);
        Task<AuthResponse> RegisterAsync(RegisterRequest dto);
    }
}
