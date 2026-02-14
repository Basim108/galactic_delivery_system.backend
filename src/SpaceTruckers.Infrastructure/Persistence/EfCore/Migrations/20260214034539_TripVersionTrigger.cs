using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpaceTruckers.Infrastructure.Persistence.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class TripVersionTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Version",
                table: "trips",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION set_trip_version() RETURNS trigger AS $$
                BEGIN
                  NEW."Version" := OLD."Version" + 1;
                  RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
                """);

            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS trip_version_trigger ON trips;
                CREATE TRIGGER trip_version_trigger
                BEFORE UPDATE ON trips
                FOR EACH ROW
                EXECUTE FUNCTION set_trip_version();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trip_version_trigger ON trips;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS set_trip_version();");

            migrationBuilder.AlterColumn<long>(
                name: "Version",
                table: "trips",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 0L);
        }
    }
}
