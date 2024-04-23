using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SampleApp.Application.Migrations
{
    /// <inheritdoc />
    public partial class Upgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LanguageCulture");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EmailMessage");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "EmailMessage",
                type: "integer",
                nullable: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SendAt",
                table: "EmailMessage",
                type: "TIMESTAMP WITHOUT TIME ZONE",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                table: "EmailMessage",
                type: "TIMESTAMP WITHOUT TIME ZONE",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "BackgroundserviceInfo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AsyncTaskOperations");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AsyncTaskOperations",
                type: "integer",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetryCount",
                table: "AsyncTaskOperations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceType",
                table: "AsyncTaskOperations",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MultilingualCulture",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultilingualCulture", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChronologyEntryTranslation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    CultureId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChronologyEntryTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChronologyEntryTranslation_ChronologyEntry_CoreId",
                        column: x => x.CoreId,
                        principalTable: "ChronologyEntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChronologyEntryTranslation_MultilingualCulture_CultureId",
                        column: x => x.CultureId,
                        principalTable: "MultilingualCulture",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultilingualGlobalTexts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CultureId = table.Column<Guid>(type: "uuid", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    ViewLevel = table.Column<int>(type: "integer", nullable: false),
                    EditLevel = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultilingualGlobalTexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultilingualGlobalTexts_MultilingualCulture_CultureId",
                        column: x => x.CultureId,
                        principalTable: "MultilingualCulture",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MultilingualCulture",
                columns: new[] { "Id", "IsActive", "IsDefault", "Name" },
                values: new object[,]
                {
                    { new Guid("023eb000-7fdf-4ef5-aa76-bc116f59ebef"), true, true, "de" },
                    { new Guid("a0af7864-0dc1-49a6-a0d8-2f29157b3801"), true, false, "en" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessage_ReferenceId_ReferenceType",
                table: "EmailMessage",
                columns: new[] { "ReferenceId", "ReferenceType" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessage_Status_SendAt",
                table: "EmailMessage",
                columns: new[] { "Status", "SendAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundserviceInfo_Name",
                table: "BackgroundserviceInfo",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsyncTaskOperations_OperationKey",
                table: "AsyncTaskOperations",
                column: "OperationKey");

            migrationBuilder.CreateIndex(
                name: "IX_AsyncTaskOperations_ReferenceId_ReferenceType",
                table: "AsyncTaskOperations",
                columns: new[] { "ReferenceId", "ReferenceType" });

            migrationBuilder.CreateIndex(
                name: "IX_AsyncTaskOperations_ReferenceId_Status",
                table: "AsyncTaskOperations",
                columns: new[] { "ReferenceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ChronologyEntryTranslation_CoreId",
                table: "ChronologyEntryTranslation",
                column: "CoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ChronologyEntryTranslation_CultureId",
                table: "ChronologyEntryTranslation",
                column: "CultureId");

            migrationBuilder.CreateIndex(
                name: "IX_MultilingualCulture_IsDefault",
                table: "MultilingualCulture",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_MultilingualCulture_Name",
                table: "MultilingualCulture",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MultilingualGlobalTexts_CultureId_Key",
                table: "MultilingualGlobalTexts",
                columns: new[] { "CultureId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChronologyEntryTranslation");

            migrationBuilder.DropTable(
                name: "MultilingualGlobalTexts");

            migrationBuilder.DropTable(
                name: "MultilingualCulture");

            migrationBuilder.DropIndex(
                name: "IX_EmailMessage_ReferenceId_ReferenceType",
                table: "EmailMessage");

            migrationBuilder.DropIndex(
                name: "IX_EmailMessage_Status_SendAt",
                table: "EmailMessage");

            migrationBuilder.DropIndex(
                name: "IX_BackgroundserviceInfo_Name",
                table: "BackgroundserviceInfo");

            migrationBuilder.DropIndex(
                name: "IX_AsyncTaskOperations_OperationKey",
                table: "AsyncTaskOperations");

            migrationBuilder.DropIndex(
                name: "IX_AsyncTaskOperations_ReferenceId_ReferenceType",
                table: "AsyncTaskOperations");

            migrationBuilder.DropIndex(
                name: "IX_AsyncTaskOperations_ReferenceId_Status",
                table: "AsyncTaskOperations");

            migrationBuilder.DropColumn(
                name: "SendAt",
                table: "EmailMessage");

            migrationBuilder.DropColumn(
                name: "SentAt",
                table: "EmailMessage");

            migrationBuilder.DropColumn(
                name: "State",
                table: "BackgroundserviceInfo");

            migrationBuilder.DropColumn(
                name: "MaxRetryCount",
                table: "AsyncTaskOperations");

            migrationBuilder.DropColumn(
                name: "ReferenceType",
                table: "AsyncTaskOperations");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "EmailMessage",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "AsyncTaskOperations",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "LanguageCulture",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageCulture", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "LanguageCulture",
                columns: new[] { "Id", "IsDefault", "IsEnabled", "Name" },
                values: new object[,]
                {
                    { new Guid("023eb000-7fdf-4ef5-aa76-bc116f59ebef"), false, false, "de-AT" },
                    { new Guid("a0af7864-0dc1-49a6-a0d8-2f29157b3801"), false, false, "en-US" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LanguageCulture_IsDefault",
                table: "LanguageCulture",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageCulture_Name",
                table: "LanguageCulture",
                column: "Name",
                unique: true);
        }
    }
}
