
using Backend.Repositories.Categorias;
using Backend.Services.Funkos;
using Backend.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Infrastructure;

/// <summary>
/// Configuración de inyección de dependencias para servicios y repositorios
/// </summary>
public static class DependencyInjectionConfiguration
{
    /// <summary>
    /// Registra todos los servicios y repositorios en el contenedor de DI
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Repositorios
        services.AddScoped<IFunkoRepository, FunkoRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        
        // Servicios
        services.AddScoped<IFunkoService, FunkoService>();
        services.AddScoped<IStorageService, FileSystemStorageService>();
        
        // Cache - Memory Local
        services.AddMemoryCache();
        
        // Configuración de errores de validación de modelo
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var mensaje = string.Join(", ", context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return new BadRequestObjectResult(new { message = mensaje });
            };
        });
        
        return services;
    }
}
