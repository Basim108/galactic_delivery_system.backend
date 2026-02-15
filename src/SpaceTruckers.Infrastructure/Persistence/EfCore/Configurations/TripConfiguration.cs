using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.Infrastructure.Persistence.EfCore;

public sealed partial class SpaceTruckersDbContext
{
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
            //
            // NOTE: We intentionally keep an explicit "Version" column (bigint) instead of using PostgreSQL's
            // system column xmin, because this model increments Version in the domain and expects it to be a
            // regular mapped property.
            builder.Property(x => x.Version)
                   .HasConversion(v => (long)v, v => checked((uint)v))
                   .HasColumnType("bigint")
                   .HasDefaultValue(0L)
                   .IsConcurrencyToken();

            builder.Property(x => x.LastReachedCheckpointIndex);

            // Mapped via backing fields.
            builder.Ignore(x => x.Checkpoints);
            builder.Ignore(x => x.StartTripRequestIds);

            // Idempotency keys for StartTrip.
            builder.Property<HashSet<string>>("_startTripRequestIds")
                   .HasColumnName("start_trip_request_ids")
                   .HasColumnType("text[]")
                   .HasConversion(v => v.ToArray(),
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
}
