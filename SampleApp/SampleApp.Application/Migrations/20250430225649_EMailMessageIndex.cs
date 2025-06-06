using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampleApp.Application.Migrations;

/// <inheritdoc />
public partial class EMailMessageIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_EmailMessage_ReferenceId",
            schema: "core",
            table: "EmailMessage",
            column: "ReferenceId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_EmailMessage_ReferenceId",
            schema: "core",
            table: "EmailMessage");
    }
}
