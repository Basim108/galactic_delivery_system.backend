using Microsoft.Extensions.DependencyInjection;
using SpaceTruckers.Application.Trips.Commands;

namespace SpaceTruckers.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSpaceTruckersApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateTripHandler>());
        return services;
    }
}
