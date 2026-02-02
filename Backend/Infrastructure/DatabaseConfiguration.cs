using Backend.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infrastructure;

/// <summary>
/// Configuración de la base de datos
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Configura el contexto de base de datos con PostgreSQL
    /// </summary>
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<Context>(options =>
        {
            options.UseInMemoryDatabase("funko_db_memory");
            options.EnableSensitiveDataLogging(); // Para desarrollo
            options.EnableDetailedErrors(); // Para desarrollo
        });
        
        return services;
    }
}
