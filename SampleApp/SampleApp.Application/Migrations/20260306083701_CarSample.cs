using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampleApp.Application.Migrations;

/// <inheritdoc />
public partial class CarSample : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Car",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Manufacturer = table.Column<string>(type: "text", nullable: true),
                Model = table.Column<string>(type: "text", nullable: true),
                Year = table.Column<int>(type: "integer", nullable: true),
                Vin = table.Column<string>(type: "text", nullable: true),
                NumberOfSeats = table.Column<int>(type: "integer", nullable: false),
                IsRegistered = table.Column<bool>(type: "boolean", nullable: false),
                CarType = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                HorsePower = table.Column<int>(type: "integer", nullable: true),
                TopSpeedKmh = table.Column<int>(type: "integer", nullable: true),
                FuelType = table.Column<string>(type: "text", nullable: true),
                EngineDisplacementLiters = table.Column<double>(type: "double precision", nullable: true),
                HasTurbo = table.Column<bool>(type: "boolean", nullable: true),
                IsHistoricRegistration = table.Column<bool>(type: "boolean", nullable: true),
                LastRestorationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                BatteryCapacityKwh = table.Column<int>(type: "integer", nullable: true),
                RangeKm = table.Column<int>(type: "integer", nullable: true),
                ChargeTimeMinutes = table.Column<int>(type: "integer", nullable: true),
                SupportsFastCharging = table.Column<bool>(type: "boolean", nullable: true),
                CombustionEngineDisplacementLiters = table.Column<double>(type: "double precision", nullable: true),
                HybridType = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Car", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Car");
    }
}
