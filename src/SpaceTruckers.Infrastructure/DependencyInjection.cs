using Microsoft.Extensions.DependencyInjection;
using SpaceTruckers.Application.Abstractions;
using SpaceTruckers.Infrastructure.Events;
using SpaceTruckers.Infrastructure.Persistence;
using SpaceTruckers.Infrastructure.Time;

namespace SpaceTruckers.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSpaceTruckersInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IDomainEventPublisher, InProcessDomainEventPublisher>();

        services.AddSingleton<IDriverRepository, InMemoryDriverRepository>();
        services.AddSingleton<IVehicleRepository, InMemoryVehicleRepository>();
        services.AddSingleton<IRouteRepository, InMemoryRouteRepository>();
        services.AddSingleton<ITripRepository, InMemoryTripRepository>();
        services.AddSingleton<IResourceBookingService, InMemoryResourceBookingService>();

        return services;
    }
}
