using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpaceTruckers.Infrastructure.Persistence.EfCore.Repositories;

namespace SpaceTruckers.Infrastructure.Persistence.EfCore;

public static class EfCoreServiceCollectionExtensions
{
    public static IServiceCollection AddSpaceTruckersEfCorePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SpaceTruckersDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString("SpaceTruckersDb");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                // Only fails when the DbContext is actually resolved.
                throw new InvalidOperationException(
                    "Connection string 'ConnectionStrings:SpaceTruckersDb' is not configured. " +
                    "Set it via environment variables (recommended) or user secrets for local development.");
            }

            options
                .UseNpgsql(connectionString, npgsql =>
                {
                    npgsql
                        .SetPostgresVersion(18, 0)
                        .MigrationsAssembly(typeof(SpaceTruckersDbContext).Assembly.FullName);
                });
        });

        services.AddScoped<EfDriverRepository>();
        services.AddScoped<EfVehicleRepository>();
        services.AddScoped<EfRouteRepository>();
        services.AddScoped<EfTripRepository>();
        services.AddScoped<EfResourceBookingService>();

        return services;
    }
}
