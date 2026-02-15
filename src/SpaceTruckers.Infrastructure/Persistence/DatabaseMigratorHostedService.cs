using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using SpaceTruckers.Application;
using SpaceTruckers.Infrastructure.Persistence.EfCore;

namespace SpaceTruckers.Infrastructure.Persistence;

public sealed class DatabaseMigratorHostedService(
    IServiceProvider serviceProvider,
    IFeatureManager featureManager,
    ILogger<DatabaseMigratorHostedService> logger)
    : IHostedService
{
    private static readonly SemaphoreSlim MigrationLock = new(1, 1);
    private static bool _migrated;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!await featureManager.IsEnabledAsync(FeatureFlags.USE_DOMAIN_PERSISTENT_STORAGE))
        {
            logger.LogInformation("UseDomainPersistentStorage is disabled; skipping database migration.");
            return;
        }

        await MigrationLock.WaitAsync(cancellationToken);
        try
        {
            if (_migrated)
            {
                return;
            }

            logger.LogInformation("UseDomainPersistentStorage is enabled; applying EF Core migrations.");

            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SpaceTruckersDbContext>();
            await db.Database.MigrateAsync(cancellationToken);

            _migrated = true;

            logger.LogInformation("EF Core migrations applied successfully.");
        }
        finally
        {
            MigrationLock.Release();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
