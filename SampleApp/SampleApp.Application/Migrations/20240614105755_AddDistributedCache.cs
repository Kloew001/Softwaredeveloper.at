using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampleApp.Application.Migrations
{
    /// <inheritdoc />
    public partial class AddDistributedCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUserAudit",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ApplicationUserAudit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationUserAudit_ApplicationUser_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "identity",
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserAudit_ApplicationUser_CreatedById",
                        column: x => x.CreatedById,
                        principalSchema: "identity",
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationUserAudit_ApplicationUser_ModifiedById",
                        column: x => x.ModifiedById,
                        principalSchema: "identity",
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserAudit_AuditId",
                schema: "audit",
                table: "ApplicationUserAudit",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserAudit_CreatedById",
                schema: "audit",
                table: "ApplicationUserAudit",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserAudit_ModifiedById",
                schema: "audit",
                table: "ApplicationUserAudit",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserAudit_TransactionId",
                schema: "audit",
                table: "ApplicationUserAudit",
                column: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserAudit",
                schema: "audit");
        }
    }
}
