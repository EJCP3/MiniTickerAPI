using System.Net;
using System.Text.Json;

namespace MiniTicker.WebApi.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Intenta ejecutar la solicitud normal
                await _next(context);
            }
            catch (Exception error)
            {
                // Si algo explota (throw), capturamos el error aquí
                var response = context.Response;
                response.ContentType = "application/json";

                switch (error)
                {
                    case InvalidOperationException: // Error de lógica (ej. Estado incorrecto)
                    case ArgumentException:         // Datos inválidos
                        response.StatusCode = (int)HttpStatusCode.BadRequest; // 400
                        break;

                    case KeyNotFoundException:      // No encontrado
                        response.StatusCode = (int)HttpStatusCode.NotFound; // 404
                        break;

                    case UnauthorizedAccessException: // Sin permiso
                        response.StatusCode = (int)HttpStatusCode.Unauthorized; // 401
                        break;

                    default: // Error inesperado (bug)
                        response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500
                        break;
                }

                var result = JsonSerializer.Serialize(new { message = error.Message });
                await response.WriteAsync(result);
            }
        }
    }
}