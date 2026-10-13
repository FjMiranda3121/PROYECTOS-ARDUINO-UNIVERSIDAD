namespace EcoRutas.Api.DTOs
{
    public record MedicionResponse(
        Guid Id,
        string CanecaId,
        int PorcentajeEstimado,
        string Estado,
        DateTime FechaRegistro
        
        );
}
