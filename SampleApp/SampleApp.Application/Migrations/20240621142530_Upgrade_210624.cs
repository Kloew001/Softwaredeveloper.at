using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampleApp.Application.Migrations
{
    /// <inheritdoc />
    public partial class Upgrade_210624 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attachment1",
                schema: "core",
                table: "EmailMessage");

            migrationBuilder.DropColumn(
                name: "Attachment1Name",
                schema: "core",
                table: "EmailMessage");

            migrationBuilder.DropColumn(
                name: "Attachment2",
                schema: "core",
                table: "EmailMessage");

            migrationBuilder.DropColumn(
                name: "Attachment2Name",
                schema: "core",
                table: "EmailMessage");

            migrationBuilder.AddColumn<Guid>(
                name: "CultureId",
                schema: "core",
                table: "EmailMessage",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TemplateId",
                schema: "core",
                table: "EmailMessage",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EMailMessageAttachment",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EMailMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    BinaryContentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMailMessageAttachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EMailMessageAttachment_BinaryContent_BinaryContentId",
                        column: x => x.BinaryContentId,
                        principalSchema: "core",
                        principalTable: "BinaryContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EMailMessageAttachment_EmailMessage_EMailMessageId",
                        column: x => x.EMailMessageId,
                        principalSchema: "core",
                        principalTable: "EmailMessage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EMailMessageConfiguration",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMailMessageConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EMailMessageConfigurationTranslation",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Style = table.Column<string>(type: "text", nullable: true),
                    Signature = table.Column<string>(type: "text", nullable: true),
                    HtmlContent = table.Column<string>(type: "text", nullable: true),
                    CoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    CultureId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMailMessageConfigurationTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EMailMessageConfigurationTranslation_EMailMessageConfigurat~",
                        column: x => x.CoreId,
                        principalSchema: "core",
                        principalTable: "EMailMessageConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EMailMessageConfigurationTranslation_MultilingualCulture_Cu~",
                        column: x => x.CultureId,
                        principalSchema: "core",
                        principalTable: "MultilingualCulture",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EMailMessageTemplate",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ConfigurationID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMailMessageTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EMailMessageTemplate_EMailMessageConfiguration_Configuratio~",
                        column: x => x.ConfigurationID,
                        principalSchema: "core",
                        principalTable: "EMailMessageConfiguration",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EMailMessageTemplateTranslation",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: true),
                    HtmlContent = table.Column<string>(type: "text", nullable: true),
                    CoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    CultureId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMailMessageTemplateTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EMailMessageTemplateTranslation_EMailMessageTemplate_CoreId",
                        column: x => x.CoreId,
                        principalSchema: "core",
                        principalTable: "EMailMessageTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EMailMessageTemplateTranslation_MultilingualCulture_Culture~",
                        column: x => x.CultureId,
                        principalSchema: "core",
                        principalTable: "MultilingualCulture",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessage_CultureId",
                schema: "core",
                table: "EmailMessage",
                column: "CultureId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessage_TemplateId",
                schema: "core",
                table: "EmailMessage",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_EMailMessageAttachment_BinaryContentId",
                schema: "core",
                table: "EMailMessageAttachment",
                column: "BinaryContentId");

            migrationBuilder.CreateIndex(
                name: "IX_EMailMessageAttachment_EMailMessageId",
                schema: "core",
                table: "EMailMessageAttachment",
                column: "EMailMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EMailMessageConfiguration_IsDefault",
                schema: "core",
                table: "EMailMessageConfiguration",
                column: "IsDefault",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EMailMessageConfigurationTranslation_CoreId",
                schema: "core",
                table: "EMailMessageConfigurationTranslation",
                column: "CoreId");

            migrationBuilder.CreateIndex(
                name: "IX_EMailMessageConfigurationTranslation_CoreId_CultureId",
                schema: "core",
                table: "EMailMessageConfigurationTranslation",
                columns: new[] { "CoreId", "CultureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EMailMessageConfigurationTranslation_CultureId",
                schema: "core",
                table: "EMailMessageConfigurationTranslation",
                column: "CultureId");

            migrationBuilder.CreateIndex(
                name: "IX_EMailMessageTemplate_ConfigurationID",
                schema: "core",
                table: "EMailMessageTemplate",
                column: "ConfigurationID");

            migrationBuilder.CreateIndex(
                name: "IX_EMailMessageTemplateTranslation_CoreId",
                schema: "core",
                table: "EMailMessageTemplateTranslation",
                column: "CoreId");

            migrationBuilder.CreateIndex(
                name: "IX_EMailMessageTemplateTranslation_CoreId_CultureId",
                schema: "core",
                table: "EMailMessageTemplateTranslation",
                columns: new[] { "CoreId", "CultureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EMailMessageTemplateTranslation_CultureId",
                schema: "core",
                table: "EMailMessageTemplateTranslation",
                column: "CultureId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailMessage_EMailMessageTemplate_TemplateId",
                schema: "core",
                table: "EmailMessage",
                column: "TemplateId",
                principalSchema: "core",
                principalTable: "EMailMessageTemplate",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailMessage_MultilingualCulture_CultureId",
                schema: "core",
                table: "EmailMessage",
                column: "CultureId",
                principalSchema: "core",
                principalTable: "MultilingualCulture",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailMessage_EMailMessageTemplate_TemplateId",
                schema: "core",
                table: "EmailMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailMessage_MultilingualCulture_CultureId",
                schema: "core",
                table: "EmailMessage");

            migrationBuilder.DropTable(
                name: "EMailMessageAttachment",
                schema: "core");

            migrationBuilder.DropTable(
                name: "EMailMessageConfigurationTranslation",
                schema: "core");

            migrationBuilder.DropTable(
                name: "EMailMessageTemplateTranslation",
                schema: "core");

            migrationBuilder.DropTable(
                name: "EMailMessageTemplate",
                schema: "core");

            migrationBuilder.DropTable(
                name: "EMailMessageConfiguration",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "IX_EmailMessage_CultureId",
                schema: "core",
                table: "EmailMessage");

            migrationBuilder.DropIndex(
                name: "IX_EmailMessage_TemplateId",
                schema: "core",
                table: "EmailMessage");

            migrationBuilder.DropColumn(
                name: "CultureId",
                schema: "core",
                table: "EmailMessage");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                schema: "core",
                table: "EmailMessage");

            migrationBuilder.AddColumn<byte[]>(
                name: "Attachment1",
                schema: "core",
                table: "EmailMessage",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Attachment1Name",
                schema: "core",
                table: "EmailMessage",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Attachment2",
                schema: "core",
                table: "EmailMessage",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Attachment2Name",
                schema: "core",
                table: "EmailMessage",
                type: "text",
                nullable: true);
        }
    }
}
