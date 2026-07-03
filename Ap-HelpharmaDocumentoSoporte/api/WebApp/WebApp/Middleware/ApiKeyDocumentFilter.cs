using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApp.Middleware
{
    public class ApiKeyHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-API-KEY",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Api Key requerida para acceder al endpoint",
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });
        }
    }
}
