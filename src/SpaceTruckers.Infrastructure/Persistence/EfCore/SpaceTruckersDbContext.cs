using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Resources;
using SpaceTruckers.Domain.Routes;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.Infrastructure.Persistence.EfCore;

public sealed class SpaceTruckersDbContext(DbContextOptions<SpaceTruckersDbContext> options) : DbContext(options)
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

    private sealed class DriverConfiguration : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.ToTable("drivers");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasConversion(v => v.Value, v => new DriverId(v))
                .ValueGeneratedNever();

            builder.Property(x => x.Name)
                .HasMaxLength(200);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasIndex(x => x.Status);
        }
    }

    private sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.ToTable("vehicles");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasConversion(v => v.Value, v => new VehicleId(v))
                .ValueGeneratedNever();

            builder.Property(x => x.Name)
                .HasMaxLength(200);

            builder.Property(x => x.Type)
                .HasMaxLength(100);

            builder.Property(x => x.CargoCapacity)
                .HasConversion(v => v.Value, v => new CargoCapacity(v));

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasIndex(x => x.Status);
        }
    }

    private sealed class RouteConfiguration : IEntityTypeConfiguration<Route>
    {
        public void Configure(EntityTypeBuilder<Route> builder)
        {
            builder.ToTable("routes");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasConversion(v => v.Value, v => new RouteId(v))
                .ValueGeneratedNever();

            builder.Property(x => x.Name)
                .HasMaxLength(200);

            // Mapped via backing field navigation.
            builder.Ignore(x => x.Checkpoints);

            builder.OwnsMany<RouteCheckpoint>("_checkpoints", checkpoints =>
            {
                checkpoints.ToTable("route_checkpoints");

                checkpoints.WithOwner().HasForeignKey("RouteId");

                checkpoints.Property<RouteId>("RouteId")
                    .HasConversion(v => v.Value, v => new RouteId(v));

                checkpoints.HasKey(x => x.Id);

                checkpoints.Property(x => x.Id)
                    .HasConversion(v => v.Value, v => new CheckpointId(v))
                    .ValueGeneratedNever();

                checkpoints.Property(x => x.Sequence);
                checkpoints.Property(x => x.Name).HasMaxLength(200);

                checkpoints.HasIndex("RouteId", nameof(RouteCheckpoint.Sequence)).IsUnique();
            });

            builder.Navigation("_checkpoints")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }

    private sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
    {
        public void Configure(EntityTypeBuilder<Trip> builder)
        {
            builder.ToTable("trips");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasConversion(v => v.Value, v => new TripId(v))
                .ValueGeneratedNever();

            builder.Property(x => x.DriverId)
                .HasConversion(v => v.Value, v => new DriverId(v));

            builder.Property(x => x.VehicleId)
                .HasConversion(v => v.Value, v => new VehicleId(v));

            builder.Property(x => x.RouteId)
                .HasConversion(v => v.Value, v => new RouteId(v));

            builder.Property(x => x.CargoRequirement)
                .HasConversion(v => v.Value, v => new CargoRequirement(v));

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Optimistic concurrency token.
            // NOTE: PostgreSQL doesn't have SQL Server's rowversion type; we implement the same behavior by
            // using the version value in the UPDATE WHERE clause.
            builder.Property(x => x.Version)
                .HasConversion(v => (long)v, v => (uint)v)
                .IsRowVersion();

            builder.Property(x => x.LastReachedCheckpointIndex);

            // Mapped via backing fields.
            builder.Ignore(x => x.Checkpoints);
            builder.Ignore(x => x.StartTripRequestIds);

            // Idempotency keys for StartTrip.
            builder.Property<HashSet<string>>("_startTripRequestIds")
                .HasColumnName("start_trip_request_ids")
                .HasColumnType("text[]")
                .HasConversion(
                    v => v.ToArray(),
                    v => new HashSet<string>(v, StringComparer.Ordinal))
                .Metadata.SetValueComparer(new ValueComparer<HashSet<string>>(
                    (a, b) => a == null
                        ? b == null
                        : b != null && a.SetEquals(b),
                    v => v == null
                        ? 0
                        : v.Aggregate(0, (acc, x) => HashCode.Combine(acc, x.GetHashCode(StringComparison.Ordinal))),
                    v => v == null
                        ? new HashSet<string>(StringComparer.Ordinal)
                        : new HashSet<string>(v, StringComparer.Ordinal)));

            builder.Ignore(x => x.RecordedEvents);
            builder.Ignore(x => x.LastReachedCheckpointName);

            builder.OwnsMany<TripCheckpoint>("_checkpoints", checkpoints =>
            {
                checkpoints.ToTable("trip_checkpoints");

                checkpoints.WithOwner().HasForeignKey("TripId");

                checkpoints.Property<TripId>("TripId")
                    .HasConversion(v => v.Value, v => new TripId(v));

                checkpoints.HasKey(x => x.Id);

                checkpoints.Property(x => x.Id)
                    .HasConversion(v => v.Value, v => new CheckpointId(v))
                    .ValueGeneratedNever();

                checkpoints.Property(x => x.Sequence);
                checkpoints.Property(x => x.Name).HasMaxLength(200);

                checkpoints.HasIndex("TripId", nameof(TripCheckpoint.Sequence)).IsUnique();
            });

            builder.Navigation("_checkpoints")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => x.DriverId);
            builder.HasIndex(x => x.VehicleId);
            builder.HasIndex(x => x.RouteId);
        }
    }

    private sealed class TripEventConfiguration : IEntityTypeConfiguration<TripEvent>
    {
        public void Configure(EntityTypeBuilder<TripEvent> builder)
        {
            builder.ToTable("trip_events");

            builder.HasKey(x => new { x.TripId, x.Sequence });

            builder.Property(x => x.TripId)
                .HasConversion(v => v.Value, v => new TripId(v))
                .ValueGeneratedNever();

            builder.Property(x => x.EventType)
                .HasMaxLength(300);

            builder.Property(x => x.OccurredAt);

            // Persist as JSONB to keep schema stable while still allowing analytics later.
            builder.Property(x => x.Payload)
                .HasColumnType("jsonb");

            builder.HasIndex(x => x.TripId);

            builder.HasOne<Trip>()
                .WithMany()
                .HasForeignKey(x => x.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    private sealed class ResourceBookingConfiguration : IEntityTypeConfiguration<ResourceBooking>
    {
        public void Configure(EntityTypeBuilder<ResourceBooking> builder)
        {
            builder.ToTable("resource_bookings");

            builder.HasKey(x => new { x.ResourceType, x.ResourceId });

            builder.Property(x => x.ResourceType)
                .HasConversion<string>()
                .HasMaxLength(25);

            builder.Property(x => x.ResourceId);
            builder.Property(x => x.TripId);

            builder.HasIndex(x => x.TripId);
        }
    }
}
