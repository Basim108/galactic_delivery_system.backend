using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace SpaceTruckers.IntegrationTests.Api;

public sealed class SpaceTruckersApiFactory : WebApplicationFactory<Program>
{
    private static readonly SemaphoreSlim StartLock = new(1, 1);
    private static bool _started;

    private static readonly PostgreSqlContainer DbContainer = new PostgreSqlBuilder("postgres:18")
        .WithDatabase("spacetruckers")
        .WithUsername("spacetruckers")
        .WithPassword("spacetruckers")
        .Build();

    public SpaceTruckersApiFactory()
    {
        EnsureDbStarted();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Ensure we run against a real database.
        builder.UseSetting("ConnectionStrings:SpaceTruckersDb", DbContainer.GetConnectionString());

        // Enable EF persistence.
        builder.UseSetting("FeatureManagement:UseDomainPersistentStorage", "true");
    }

    private static void EnsureDbStarted()
    {
        if (_started)
        {
            return;
        }

        StartLock.Wait();
        try
        {
            if (_started)
            {
                return;
            }

            DbContainer.StartAsync().GetAwaiter().GetResult();
            _started = true;
        }
        finally
        {
            StartLock.Release();
        }
    }
}
