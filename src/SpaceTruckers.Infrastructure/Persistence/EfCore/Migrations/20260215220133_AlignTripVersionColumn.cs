using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpaceTruckers.Infrastructure.Persistence.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class AlignTripVersionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Snapshot-only migration.
            //
            // Earlier migrations already created the explicit trips.Version column used for optimistic concurrency.
            // This migration only updates the EF model snapshot to match the current model.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Snapshot-only migration (no-op).
        }
    }
}
