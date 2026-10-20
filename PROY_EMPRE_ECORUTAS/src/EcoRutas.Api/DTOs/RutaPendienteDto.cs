namespace EcoRutas.Api.DTOs
{
    public record RutaPendienteDto(
    string Id,
    string Ubicacion,
    double Latitud,
    double Longitud,
    string EstadoActual,
    double PorcentajeLlenado
);
}
