using Data.Dependences;
using Business.Interface;


namespace WebApp.DependencyContainer
{
    public static class DependencyContainer
    {
        public static IServiceCollection DependencyInjection(this IServiceCollection services)
        {
           // services.AddScoped<IUsuarioValidation, UsuarioValidation>();

            services.DataDependencyInjectionAccess();
            services.BusDependencyInjectionAccess();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader()
                                      .WithOrigins("*"));
            });

            return services;
        }
    }
}
