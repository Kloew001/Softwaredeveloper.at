using Microsoft.EntityFrameworkCore.Migrations;

namespace SampleApp.Application.Migrations;

public partial class DistributedLock : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DistributedLock",
            schema: "core",
            columns: table => new
            {
                LockId = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                Owner = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DistributedLock", x => x.LockId);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DistributedLock",
            schema: "core");
    }
}
