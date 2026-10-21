namespace EcoRutas.Api.DTOs
{
    public record MedicionResponse(
        Guid Id,
        string CanecaId,
        int PorcentajeEstimado,
        double DistanciaCm,
        string Estado,
        DateTime FechaRegistro
        
        );
}
