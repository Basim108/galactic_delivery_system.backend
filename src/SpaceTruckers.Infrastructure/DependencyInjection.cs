using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Infrastructure.Events;
using SpaceTruckers.Infrastructure.Persistence;
using SpaceTruckers.Infrastructure.Persistence.EfCore;
using SpaceTruckers.Infrastructure.Time;

namespace SpaceTruckers.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSpaceTruckersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IDomainEventPublisher, InProcessDomainEventPublisher>();

        // In-memory implementations (always available).
        services.AddSingleton<InMemoryDriverRepository>();
        services.AddSingleton<InMemoryVehicleRepository>();
        services.AddSingleton<InMemoryRouteRepository>();
        services.AddSingleton<InMemoryTripRepository>();
        services.AddSingleton<InMemoryResourceBookingService>();

        // EF Core implementations (registered; they will only be resolved when the feature flag is enabled).
        services.AddSpaceTruckersEfCorePersistence(configuration);

        // Feature-flag routing ports.
        services.AddScoped<IDriverRepository, FeatureFlaggedDriverRepository>();
        services.AddScoped<IVehicleRepository, FeatureFlaggedVehicleRepository>();
        services.AddScoped<IRouteRepository, FeatureFlaggedRouteRepository>();
        services.AddScoped<ITripRepository, FeatureFlaggedTripRepository>();
        services.AddScoped<IResourceBookingService, FeatureFlaggedResourceBookingService>();

        // Apply migrations at startup (only when persistent storage feature is enabled).
        services.AddHostedService<DatabaseMigratorHostedService>();

        return services;
    }
}
