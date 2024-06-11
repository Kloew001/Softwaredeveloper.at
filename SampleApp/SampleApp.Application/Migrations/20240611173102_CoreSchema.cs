using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampleApp.Application.Migrations
{
    /// <inheritdoc />
    public partial class CoreSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsyncTaskOperations_ApplicationUser_ExecuteById",
                table: "AsyncTaskOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MultilingualGlobalTexts_MultilingualCulture_CultureId",
                table: "MultilingualGlobalTexts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MultilingualGlobalTexts",
                table: "MultilingualGlobalTexts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AsyncTaskOperations",
                table: "AsyncTaskOperations");

            migrationBuilder.DeleteData(
                table: "Person",
                keyColumn: "Id",
                keyValue: new Guid("144fcd4a-3b46-4ff4-ad82-734d037f3e2d"));

            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.RenameTable(
                name: "MultilingualCulture",
                newName: "MultilingualCulture",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "EmailMessage",
                newName: "EmailMessage",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "BinaryContent",
                newName: "BinaryContent",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "BackgroundserviceInfo",
                newName: "BackgroundserviceInfo",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "MultilingualGlobalTexts",
                newName: "MultilingualGlobalText",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "AsyncTaskOperations",
                newName: "AsyncTaskOperation",
                newSchema: "core");

            migrationBuilder.RenameIndex(
                name: "IX_MultilingualGlobalTexts_CultureId_Key",
                schema: "core",
                table: "MultilingualGlobalText",
                newName: "IX_MultilingualGlobalText_CultureId_Key");

            migrationBuilder.RenameIndex(
                name: "IX_AsyncTaskOperations_ReferenceId_Status",
                schema: "core",
                table: "AsyncTaskOperation",
                newName: "IX_AsyncTaskOperation_ReferenceId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_AsyncTaskOperations_ReferenceId_ReferenceType",
                schema: "core",
                table: "AsyncTaskOperation",
                newName: "IX_AsyncTaskOperation_ReferenceId_ReferenceType");

            migrationBuilder.RenameIndex(
                name: "IX_AsyncTaskOperations_OperationKey",
                schema: "core",
                table: "AsyncTaskOperation",
                newName: "IX_AsyncTaskOperation_OperationKey");

            migrationBuilder.RenameIndex(
                name: "IX_AsyncTaskOperations_ExecuteById",
                schema: "core",
                table: "AsyncTaskOperation",
                newName: "IX_AsyncTaskOperation_ExecuteById");

            //migrationBuilder.RenameIndex(
            //    name: "IX_AsyncTaskOperations_ExecuteAt_Status",
            //    schema: "core",
            //    table: "AsyncTaskOperation",
            //    newName: "IX_AsyncTaskOperation_ExecuteAt_Status");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MultilingualGlobalText",
                schema: "core",
                table: "MultilingualGlobalText",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AsyncTaskOperation",
                schema: "core",
                table: "AsyncTaskOperation",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AsyncTaskOperation_ApplicationUser_ExecuteById",
                schema: "core",
                table: "AsyncTaskOperation",
                column: "ExecuteById",
                principalSchema: "identity",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MultilingualGlobalText_MultilingualCulture_CultureId",
                schema: "core",
                table: "MultilingualGlobalText",
                column: "CultureId",
                principalSchema: "core",
                principalTable: "MultilingualCulture",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsyncTaskOperation_ApplicationUser_ExecuteById",
                schema: "core",
                table: "AsyncTaskOperation");

            migrationBuilder.DropForeignKey(
                name: "FK_MultilingualGlobalText_MultilingualCulture_CultureId",
                schema: "core",
                table: "MultilingualGlobalText");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MultilingualGlobalText",
                schema: "core",
                table: "MultilingualGlobalText");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AsyncTaskOperation",
                schema: "core",
                table: "AsyncTaskOperation");

            migrationBuilder.RenameTable(
                name: "MultilingualCulture",
                schema: "core",
                newName: "MultilingualCulture");

            migrationBuilder.RenameTable(
                name: "EmailMessage",
                schema: "core",
                newName: "EmailMessage");

            migrationBuilder.RenameTable(
                name: "BinaryContent",
                schema: "core",
                newName: "BinaryContent");

            migrationBuilder.RenameTable(
                name: "BackgroundserviceInfo",
                schema: "core",
                newName: "BackgroundserviceInfo");

            migrationBuilder.RenameTable(
                name: "MultilingualGlobalText",
                schema: "core",
                newName: "MultilingualGlobalTexts");

            migrationBuilder.RenameTable(
                name: "AsyncTaskOperation",
                schema: "core",
                newName: "AsyncTaskOperations");

            migrationBuilder.RenameIndex(
                name: "IX_MultilingualGlobalText_CultureId_Key",
                table: "MultilingualGlobalTexts",
                newName: "IX_MultilingualGlobalTexts_CultureId_Key");

            migrationBuilder.RenameIndex(
                name: "IX_AsyncTaskOperation_ReferenceId_Status",
                table: "AsyncTaskOperations",
                newName: "IX_AsyncTaskOperations_ReferenceId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_AsyncTaskOperation_ReferenceId_ReferenceType",
                table: "AsyncTaskOperations",
                newName: "IX_AsyncTaskOperations_ReferenceId_ReferenceType");

            migrationBuilder.RenameIndex(
                name: "IX_AsyncTaskOperation_OperationKey",
                table: "AsyncTaskOperations",
                newName: "IX_AsyncTaskOperations_OperationKey");

            migrationBuilder.RenameIndex(
                name: "IX_AsyncTaskOperation_ExecuteById",
                table: "AsyncTaskOperations",
                newName: "IX_AsyncTaskOperations_ExecuteById");

            migrationBuilder.RenameIndex(
                name: "IX_AsyncTaskOperation_ExecuteAt_Status",
                table: "AsyncTaskOperations",
                newName: "IX_AsyncTaskOperations_ExecuteAt_Status");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MultilingualGlobalTexts",
                table: "MultilingualGlobalTexts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AsyncTaskOperations",
                table: "AsyncTaskOperations",
                column: "Id");

            migrationBuilder.InsertData(
                table: "Person",
                columns: new[] { "Id", "FirstName", "IsDeleted", "LastName" },
                values: new object[] { new Guid("144fcd4a-3b46-4ff4-ad82-734d037f3e2d"), "Huber", false, "Tester" });

            migrationBuilder.AddForeignKey(
                name: "FK_AsyncTaskOperations_ApplicationUser_ExecuteById",
                table: "AsyncTaskOperations",
                column: "ExecuteById",
                principalSchema: "identity",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MultilingualGlobalTexts_MultilingualCulture_CultureId",
                table: "MultilingualGlobalTexts",
                column: "CultureId",
                principalTable: "MultilingualCulture",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
