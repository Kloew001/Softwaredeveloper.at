using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampleApp.Application.Migrations;

/// <inheritdoc />
public partial class Audit_ValidFrom : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "ValidTo",
            schema: "audit",
            table: "PersonAudit",
            newName: "AuditValidTo");

        migrationBuilder.RenameColumn(
            name: "ValidFrom",
            schema: "audit",
            table: "PersonAudit",
            newName: "AuditValidFrom");

        migrationBuilder.AlterColumn<DateTime>(
            name: "AuditValidFrom",
            schema: "audit",
            table: "PersonAudit",
            type: "TIMESTAMP WITHOUT TIME ZONE",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.RenameColumn(
            name: "ValidTo",
            schema: "audit",
            table: "ApplicationUserAudit",
            newName: "AuditValidTo");

        migrationBuilder.RenameColumn(
            name: "ValidFrom",
            schema: "audit",
            table: "ApplicationUserAudit",
            newName: "AuditValidFrom");

        migrationBuilder.AlterColumn<DateTime>(
            name: "AuditValidFrom",
            schema: "audit",
            table: "ApplicationUserAudit",
            type: "TIMESTAMP WITHOUT TIME ZONE",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.CreateIndex(
            name: "IX_AsyncTaskOperation_CreatedAt",
            schema: "core",
            table: "AsyncTaskOperation",
            column: "CreatedAt");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_AsyncTaskOperation_CreatedAt",
            schema: "core",
            table: "AsyncTaskOperation");

        migrationBuilder.DropColumn(
            name: "AuditValidFrom",
            schema: "audit",
            table: "PersonAudit");

        migrationBuilder.DropColumn(
            name: "AuditValidFrom",
            schema: "audit",
            table: "ApplicationUserAudit");

        migrationBuilder.RenameColumn(
            name: "AuditValidTo",
            schema: "audit",
            table: "PersonAudit",
            newName: "ValidTo");

        migrationBuilder.RenameColumn(
            name: "AuditValidTo",
            schema: "audit",
            table: "ApplicationUserAudit",
            newName: "ValidTo");

        migrationBuilder.AddColumn<DateTime>(
            name: "ValidFrom",
            schema: "audit",
            table: "PersonAudit",
            type: "TIMESTAMP WITHOUT TIME ZONE",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "ValidFrom",
            schema: "audit",
            table: "ApplicationUserAudit",
            type: "TIMESTAMP WITHOUT TIME ZONE",
            nullable: true);
    }
}
