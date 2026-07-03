using Data;
using Data.DocSoporte;
using Microsoft.OpenApi.Models;
using WebApp.DependencyContainer;
using WebApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// Servicios
// ----------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<DocumentoSoporteOfimaData>();

builder.Services.DependencyInjection();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "ApiKey requerida",
        In = ParameterLocation.Header,
        Name = "X-API-KEY",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// ----------------------
// Pipeline
// ----------------------

// Swagger disponible también en producción
app.UseSwagger();
app.UseSwaggerUI();

// Evitar que ApiKey bloquee Swagger
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/swagger"),
    appBuilder =>
    {
        appBuilder.UseMiddleware<ApiKeyMiddleware>();
    });

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();