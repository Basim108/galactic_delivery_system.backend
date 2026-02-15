using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Trips;

namespace SpaceTruckers.Infrastructure.Persistence.EfCore;

public sealed partial class SpaceTruckersDbContext
{
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
}
