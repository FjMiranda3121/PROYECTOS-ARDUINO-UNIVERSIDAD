using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EcoRutas.Api.Security;

// Esta etiqueta nos permite usar [ApiKey] arriba de cualquier endpoint
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAttribute : Attribute, IAsyncActionFilter
{
    private const string API_KEY_HEADER_NAME = "X-Api-Key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. Verificamos si la petición trae la "tarjeta" (El header)
        if (!context.HttpContext.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
        {
            context.Result = new UnauthorizedObjectResult(new ProblemDetails
            {
                Type = "https://ecorutas.com/errors/unauthorized",
                Title = "No autorizado",
                Detail = "Falta el encabezado X-Api-Key en la petición.",
                Status = StatusCodes.Status401Unauthorized
            });
            return;
        }

        // 2. Extraemos la contraseña maestra de nuestro appsettings.json
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var validApiKey = configuration.GetValue<string>("EcoRutasSettings:ApiKey");

        // 3. Comparamos las llaves
        if (!validApiKey!.Equals(extractedApiKey))
        {
            context.Result = new UnauthorizedObjectResult(new ProblemDetails
            {
                Type = "https://ecorutas.com/errors/unauthorized",
                Title = "Acceso denegado",
                Detail = "La API Key proporcionada es inválida.",
                Status = StatusCodes.Status401Unauthorized
            });
            return;
        }

        // 4. Si la llave es correcta, dejamos que la petición continúe hacia el Controlador
        await next();
    }
}