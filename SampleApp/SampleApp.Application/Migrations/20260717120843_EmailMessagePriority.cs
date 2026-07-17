using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampleApp.Application.Migrations
{
    /// <inheritdoc />
    public partial class EmailMessagePriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailMessage_Status_SendAt",
                schema: "core",
                table: "EmailMessage");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "core",
                table: "EmailMessage",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessage_Status_Priority_SendAt",
                schema: "core",
                table: "EmailMessage",
                columns: new[] { "Status", "Priority", "SendAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailMessage_Status_Priority_SendAt",
                schema: "core",
                table: "EmailMessage");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "core",
                table: "EmailMessage");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessage_Status_SendAt",
                schema: "core",
                table: "EmailMessage",
                columns: new[] { "Status", "SendAt" });
        }
    }
}
