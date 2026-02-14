using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SpaceTruckers.Infrastructure.Persistence.EfCore;

public sealed partial class SpaceTruckersDbContext
{
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
