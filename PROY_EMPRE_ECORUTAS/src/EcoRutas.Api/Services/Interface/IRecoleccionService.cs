using EcoRutas.Api.DTOs;

namespace EcoRutas.Api.Services.Interface
{
    public interface IRecoleccionService
    {
        Task<IEnumerable<RutaPendienteDto>> ObtenerRutasPendientesAsync();
        Task<RecoleccionResponse> RegistrarRecoleccionAsync(RecoleccionCreateDto dto, Guid usuarioId);
        Task<List<RutaOperativaResponse>> ObtenerRutaOperativaAsync();
        Task<List<RecoleccionResponse>> ObtenerHistorialAsync(string? canecaId = null);
    }
}
