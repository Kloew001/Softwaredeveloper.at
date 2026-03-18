using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampleApp.Application.Migrations;

[DbContext(typeof(SampleAppContext))]
[Migration("20260318160000_BinaryContentDataSplit")]
public partial class BinaryContentDataSplit : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "BinaryContentData",
            schema: "core",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                BinaryContentId = table.Column<Guid>(type: "uuid", nullable: false),
                Bytes = table.Column<byte[]>(type: "bytea", nullable: true),
                ExtractedText = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BinaryContentData", x => x.Id);
                table.ForeignKey(
                    name: "FK_BinaryContentData_BinaryContent_BinaryContentId",
                    column: x => x.BinaryContentId,
                    principalSchema: "core",
                    principalTable: "BinaryContent",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_BinaryContentData_BinaryContentId",
            schema: "core",
            table: "BinaryContentData",
            column: "BinaryContentId",
            unique: true);

        migrationBuilder.Sql("""
            INSERT INTO core."BinaryContentData" ("Id", "BinaryContentId", "Bytes", "ExtractedText")
            SELECT bc."Id", bc."Id", bc."Content", bc."ExtractedText"
            FROM core."BinaryContent" bc;
            """);

        migrationBuilder.DropColumn(
            name: "Content",
            schema: "core",
            table: "BinaryContent");

        migrationBuilder.DropColumn(
            name: "ExtractedText",
            schema: "core",
            table: "BinaryContent");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<byte[]>(
            name: "Content",
            schema: "core",
            table: "BinaryContent",
            type: "bytea",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ExtractedText",
            schema: "core",
            table: "BinaryContent",
            type: "text",
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE core."BinaryContent" bc
            SET "Content" = bcd."Bytes",
                "ExtractedText" = bcd."ExtractedText"
            FROM core."BinaryContentData" bcd
            WHERE bcd."BinaryContentId" = bc."Id";
            """);

        migrationBuilder.DropTable(
            name: "BinaryContentData",
            schema: "core");
    }
}
