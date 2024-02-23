using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SampleApp.Application.Migrations
{
    /// <inheritdoc />
    public partial class Person : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "ApplicationRole",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUser",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BackgroundserviceInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: false),
                    LastFinishedAt = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    NextExecuteAt = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    LastErrorAt = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    LastErrorMessage = table.Column<string>(type: "text", nullable: true),
                    LastErrorStack = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundserviceInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailMessage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceType = table.Column<string>(type: "text", nullable: true),
                    AnAdress = table.Column<string>(type: "text", nullable: true),
                    CcAdress = table.Column<string>(type: "text", nullable: true),
                    BccAdress = table.Column<string>(type: "text", nullable: true),
                    Subject = table.Column<string>(type: "text", nullable: true),
                    HtmlContent = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    Attachment1Name = table.Column<string>(type: "text", nullable: true),
                    Attachment1 = table.Column<byte[]>(type: "bytea", nullable: true),
                    Attachment2Name = table.Column<string>(type: "text", nullable: true),
                    Attachment2 = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailMessage", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationRoleClaim",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationRoleClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationRoleClaim_ApplicationRole_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "identity",
                        principalTable: "ApplicationRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUserClaim",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationUserClaim_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUserLogin",
                schema: "identity",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserLogin", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_ApplicationUserLogin_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUserRole",
                schema: "identity",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserRole", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserRole_ApplicationRole_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "identity",
                        principalTable: "ApplicationRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserRole_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUserToken",
                schema: "identity",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserToken", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_ApplicationUserToken_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AsyncTaskOperations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationHandlerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationKey = table.Column<string>(type: "text", nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParameterSerialized = table.Column<string>(type: "text", nullable: true),
                    ExecuteById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: false),
                    ExecuteAt = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SortIndex = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsyncTaskOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsyncTaskOperations_ApplicationUser_ExecuteById",
                        column: x => x.ExecuteById,
                        principalSchema: "identity",
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BinaryContent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    MimeType = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<byte[]>(type: "bytea", nullable: true),
                    ContentSize = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceType = table.Column<string>(type: "text", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: false, defaultValueSql: "NOW()"),
                    DateModified = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: false, defaultValueSql: "NOW()"),
                    ModifiedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinaryContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BinaryContent_ApplicationUser_CreatedById",
                        column: x => x.CreatedById,
                        principalSchema: "identity",
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BinaryContent_ApplicationUser_ModifiedById",
                        column: x => x.ModifiedById,
                        principalSchema: "identity",
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChronologyEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceType = table.Column<string>(type: "text", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: false, defaultValueSql: "NOW()"),
                    DateModified = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: false, defaultValueSql: "NOW()"),
                    ModifiedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChronologyEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChronologyEntry_ApplicationUser_CreatedById",
                        column: x => x.CreatedById,
                        principalSchema: "identity",
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChronologyEntry_ApplicationUser_ModifiedById",
                        column: x => x.ModifiedById,
                        principalSchema: "identity",
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "LanguageCulture",
                columns: new[] { "Id", "IsDefault", "IsEnabled", "Name" },
                values: new object[,]
                {
                    { new Guid("023eb000-7fdf-4ef5-aa76-bc116f59ebef"), false, false, "de-AT" },
                    { new Guid("a0af7864-0dc1-49a6-a0d8-2f29157b3801"), false, false, "en-US" }
                });

            migrationBuilder.InsertData(
                table: "Person",
                columns: new[] { "Id", "FirstName", "IsDeleted", "LastName" },
                values: new object[] { new Guid("144fcd4a-3b46-4ff4-ad82-734d037f3e2d"), "Huber", false, "Tester" });

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "identity",
                table: "ApplicationRole",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoleClaim_RoleId",
                schema: "identity",
                table: "ApplicationRoleClaim",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "identity",
                table: "ApplicationUser",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "identity",
                table: "ApplicationUser",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserClaim_UserId",
                schema: "identity",
                table: "ApplicationUserClaim",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserLogin_UserId",
                schema: "identity",
                table: "ApplicationUserLogin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserRole_RoleId",
                schema: "identity",
                table: "ApplicationUserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AsyncTaskOperations_ExecuteAt_Status",
                table: "AsyncTaskOperations",
                columns: new[] { "ExecuteAt", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AsyncTaskOperations_ExecuteById",
                table: "AsyncTaskOperations",
                column: "ExecuteById");

            migrationBuilder.CreateIndex(
                name: "IX_BinaryContent_CreatedById",
                table: "BinaryContent",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BinaryContent_ModifiedById",
                table: "BinaryContent",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_BinaryContent_ReferenceId_ReferenceType",
                table: "BinaryContent",
                columns: new[] { "ReferenceId", "ReferenceType" });

            migrationBuilder.CreateIndex(
                name: "IX_ChronologyEntry_CreatedById",
                table: "ChronologyEntry",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ChronologyEntry_ModifiedById",
                table: "ChronologyEntry",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ChronologyEntry_ReferenceId_ReferenceType",
                table: "ChronologyEntry",
                columns: new[] { "ReferenceId", "ReferenceType" });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationRoleClaim",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "ApplicationUserClaim",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "ApplicationUserLogin",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "ApplicationUserRole",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "ApplicationUserToken",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "AsyncTaskOperations");

            migrationBuilder.DropTable(
                name: "BackgroundserviceInfo");

            migrationBuilder.DropTable(
                name: "BinaryContent");

            migrationBuilder.DropTable(
                name: "ChronologyEntry");

            migrationBuilder.DropTable(
                name: "EmailMessage");

            migrationBuilder.DropTable(
                name: "LanguageCulture");

            migrationBuilder.DropTable(
                name: "Person");

            migrationBuilder.DropTable(
                name: "ApplicationRole",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "ApplicationUser",
                schema: "identity");
        }
    }
}
