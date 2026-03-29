using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampleApp.Application.Migrations;

/// <inheritdoc />
public partial class PersonAdresses : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Adress",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                IsMain = table.Column<bool>(type: "boolean", nullable: false),
                Street = table.Column<string>(type: "text", nullable: true),
                City = table.Column<string>(type: "text", nullable: true),
                ZipCode = table.Column<string>(type: "text", nullable: true),
                PersonId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Adress", x => x.Id);
                table.ForeignKey(
                    name: "FK_Adress_Person_PersonId",
                    column: x => x.PersonId,
                    principalTable: "Person",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Adress_PersonId",
            table: "Adress",
            column: "PersonId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Adress");
    }
}
