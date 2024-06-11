using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampleApp.Application.Migrations
{
    /// <inheritdoc />
    public partial class PersonAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MultilingualCulture_IsDefault",
                table: "MultilingualCulture");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ChronologyEntry");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "ChronologyEntryTranslation",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                schema: "identity",
                table: "ApplicationUser",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PreferedCultureId",
                schema: "identity",
                table: "ApplicationUser",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PersonAudit",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: false, defaultValueSql: "NOW()"),
                    DateModified = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: false, defaultValueSql: "NOW()"),
                    ModifiedById = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<string>(type: "text", nullable: true),
                    AuditDate = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: false),
                    AuditAction = table.Column<string>(type: "text", nullable: true),
                    CallingMethod = table.Column<string>(type: "text", nullable: true),
                    MachineName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonAudit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonAudit_ApplicationUser_CreatedById",
                        column: x => x.CreatedById,
                        principalSchema: "identity",
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonAudit_ApplicationUser_ModifiedById",
                        column: x => x.ModifiedById,
                        principalSchema: "identity",
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonAudit_Person_AuditId",
                        column: x => x.AuditId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MultilingualCulture_IsDefault",
                table: "MultilingualCulture",
                column: "IsDefault",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChronologyEntryTranslation_CoreId_CultureId",
                table: "ChronologyEntryTranslation",
                columns: new[] { "CoreId", "CultureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUser_PreferedCultureId",
                schema: "identity",
                table: "ApplicationUser",
                column: "PreferedCultureId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonAudit_AuditId",
                schema: "audit",
                table: "PersonAudit",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonAudit_CreatedById",
                schema: "audit",
                table: "PersonAudit",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PersonAudit_ModifiedById",
                schema: "audit",
                table: "PersonAudit",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_PersonAudit_TransactionId",
                schema: "audit",
                table: "PersonAudit",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUser_MultilingualCulture_PreferedCultureId",
                schema: "identity",
                table: "ApplicationUser",
                column: "PreferedCultureId",
                principalTable: "MultilingualCulture",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUser_MultilingualCulture_PreferedCultureId",
                schema: "identity",
                table: "ApplicationUser");

            migrationBuilder.DropTable(
                name: "PersonAudit",
                schema: "audit");

            migrationBuilder.DropIndex(
                name: "IX_MultilingualCulture_IsDefault",
                table: "MultilingualCulture");

            migrationBuilder.DropIndex(
                name: "IX_ChronologyEntryTranslation_CoreId_CultureId",
                table: "ChronologyEntryTranslation");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUser_PreferedCultureId",
                schema: "identity",
                table: "ApplicationUser");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "ChronologyEntryTranslation");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                schema: "identity",
                table: "ApplicationUser");

            migrationBuilder.DropColumn(
                name: "PreferedCultureId",
                schema: "identity",
                table: "ApplicationUser");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ChronologyEntry",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MultilingualCulture_IsDefault",
                table: "MultilingualCulture",
                column: "IsDefault");
        }
    }
}
