namespace WebApp.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private const string HEADER_NAME = "X-API-KEY";

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ✅ Permitir Swagger sin validación
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            // 🔐 Obtener ApiKey configurada
            var apiKey = _configuration["ApiSecurity:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("ApiKey no configurada en el servidor.");
                return;
            }

            // 🔎 Validar que venga el header
            if (!context.Request.Headers.TryGetValue(HEADER_NAME, out var extractedKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Api Key requerida.");
                return;
            }

            // 🔐 Validar coincidencia
            if (!string.Equals(apiKey, extractedKey, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Api Key inválida.");
                return;
            }

            await _next(context);
        }
    }
}
