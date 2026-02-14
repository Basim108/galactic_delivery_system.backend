using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Routes;

namespace SpaceTruckers.Infrastructure.Persistence.EfCore;

public sealed partial class SpaceTruckersDbContext
{
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
}
