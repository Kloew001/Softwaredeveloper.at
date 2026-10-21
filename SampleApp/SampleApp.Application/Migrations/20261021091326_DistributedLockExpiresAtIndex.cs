using Microsoft.EntityFrameworkCore.Migrations;


namespace SampleApp.Application.Migrations;

/// <inheritdoc />
public partial class DistributedLockExpiresAtIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_DistributedLock_ExpiresAt",
            schema: "core",
            table: "DistributedLock",
            column: "ExpiresAt");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_DistributedLock_ExpiresAt",
            schema: "core",
            table: "DistributedLock");
    }
}
