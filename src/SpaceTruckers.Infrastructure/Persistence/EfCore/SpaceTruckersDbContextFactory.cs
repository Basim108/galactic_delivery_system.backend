using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SpaceTruckers.Infrastructure.Persistence.EfCore;

public sealed class SpaceTruckersDbContextFactory : IDesignTimeDbContextFactory<SpaceTruckersDbContext>
{
    public SpaceTruckersDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__SpaceTruckersDb") ??
            Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__SPACETRUCKERSDB");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string env var is not set. Provide ConnectionStrings__SpaceTruckersDb when running dotnet-ef.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<SpaceTruckersDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsql => npgsql.SetPostgresVersion(18, 0));

        return new SpaceTruckersDbContext(optionsBuilder.Options);
    }
}
