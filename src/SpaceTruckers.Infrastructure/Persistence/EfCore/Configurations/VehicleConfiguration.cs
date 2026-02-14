using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceTruckers.Domain.Ids;
using SpaceTruckers.Domain.Resources;

namespace SpaceTruckers.Infrastructure.Persistence.EfCore;

public sealed partial class SpaceTruckersDbContext
{
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
}
