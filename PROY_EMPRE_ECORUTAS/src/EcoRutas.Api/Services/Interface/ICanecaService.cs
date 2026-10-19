using EcoRutas.Api.DTOs;

namespace EcoRutas.Api.Services.Interface
{
    public interface ICanecaService
    {
        Task<List<CanecaResponse>> ObtenerTodasAsync(bool soloActivas = true);
        Task<CanecaResponse?> ObtenerPorIdAsync(string id);
        Task<CanecaResponse> CrearAsync(CanecaCreateDto dto);
        Task<CanecaResponse?> ActualizarAsync(string id, CanecaUpdateDto dto);

        Task<bool> EliminarAsync(string id);
    }
}
