using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampleApp.Application.Migrations;

/// <inheritdoc />
public partial class BGServiceHearthbeat : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_EmailMessage_ReferenceId",
            schema: "core",
            table: "EmailMessage");

        migrationBuilder.AddColumn<DateTime>(
            name: "LastHeartbeat",
            schema: "core",
            table: "BackgroundserviceInfo",
            type: "TIMESTAMP WITHOUT TIME ZONE",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LastHeartbeat",
            schema: "core",
            table: "BackgroundserviceInfo");

        migrationBuilder.CreateIndex(
            name: "IX_EmailMessage_ReferenceId",
            schema: "core",
            table: "EmailMessage",
            column: "ReferenceId");
    }
}