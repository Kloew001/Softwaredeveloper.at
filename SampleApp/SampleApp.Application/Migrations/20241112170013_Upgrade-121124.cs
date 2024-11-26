using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SampleApp.Application.Migrations;

/// <inheritdoc />
public partial class Upgrade121124 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ExtractedText",
            schema: "core",
            table: "BinaryContent",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "ExtractionHandledAt",
            schema: "core",
            table: "BinaryContent",
            type: "TIMESTAMP WITHOUT TIME ZONE",
            nullable: true);

        migrationBuilder.InsertData(
            schema: "core",
            table: "MultilingualGlobalText",
            columns: new[] { "Id", "CultureId", "EditLevel", "Index", "Key", "Text", "ViewLevel" },
            values: new object[,]
            {
                { new Guid("67be8513-7acd-41ed-9cf6-bc554dfa90dc"), new Guid("023eb000-7fdf-4ef5-aa76-bc116f59ebef"), 0, 0, "ValidationError.Message", "Validierungsfehler sind aufgetreten.", 0 },
                { new Guid("72a7fd7a-36b2-4f3c-b699-595392de0465"), new Guid("a0af7864-0dc1-49a6-a0d8-2f29157b3801"), 0, 0, "ValidationError.Message", "Validation error occurred.", 0 }
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            schema: "core",
            table: "MultilingualGlobalText",
            keyColumn: "Id",
            keyValue: new Guid("67be8513-7acd-41ed-9cf6-bc554dfa90dc"));

        migrationBuilder.DeleteData(
            schema: "core",
            table: "MultilingualGlobalText",
            keyColumn: "Id",
            keyValue: new Guid("72a7fd7a-36b2-4f3c-b699-595392de0465"));

        migrationBuilder.DropColumn(
            name: "ExtractedText",
            schema: "core",
            table: "BinaryContent");

        migrationBuilder.DropColumn(
            name: "ExtractionHandledAt",
            schema: "core",
            table: "BinaryContent");
    }
}
