using Microsoft.EntityFrameworkCore;
using SpaceTruckers.Domain.Resources;
using SpaceTruckers.Domain.Routes;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.Infrastructure.Persistence.EfCore;

public sealed partial class SpaceTruckersDbContext(DbContextOptions<SpaceTruckersDbContext> options) : DbContext(options)
{
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Trip> Trips => Set<Trip>();

    internal DbSet<ResourceBooking> ResourceBookings => Set<ResourceBooking>();
    internal DbSet<TripEvent> TripEvents => Set<TripEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DriverConfiguration());
        modelBuilder.ApplyConfiguration(new VehicleConfiguration());
        modelBuilder.ApplyConfiguration(new RouteConfiguration());
        modelBuilder.ApplyConfiguration(new TripConfiguration());
        modelBuilder.ApplyConfiguration(new TripEventConfiguration());
        modelBuilder.ApplyConfiguration(new ResourceBookingConfiguration());
    }
}
