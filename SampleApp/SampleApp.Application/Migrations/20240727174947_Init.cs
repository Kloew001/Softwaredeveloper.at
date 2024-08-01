using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SampleApp.Application.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "core");

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
                name: "BackgroundserviceInfo",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<int>(type: "integer", nullable: false),
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
                name: "MultilingualCulture",
                schema: "core",
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
                name: "ApplicationUser",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
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
                    PreferedCultureId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_ApplicationUser_MultilingualCulture_PreferedCultureId",
                        column: x => x.PreferedCultureId,
                        principalSchema: "core",
                        principalTable: "MultilingualCulture",
                        principalColumn: "Id");
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
                name: "MultilingualGlobalText",
                schema: "core",
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
                    table.PrimaryKey("PK_MultilingualGlobalText", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultilingualGlobalText_MultilingualCulture_CultureId",
                        column: x => x.CultureId,
                        principalSchema: "core",
                        principalTable: "MultilingualCulture",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailMessage",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: false),
                    SendAt = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceType = table.Column<string>(type: "text", nullable: true),
                    CultureId = table.Column<Guid>(type: "uuid", nullable: true),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    AnAdress = table.Column<string>(type: "text", nullable: true),
                    CcAdress = table.Column<string>(type: "text", nullable: true),
                    BccAdress = table.Column<string>(type: "text", nullable: true),
                    Subject = table.Column<string>(type: "text", nullable: true),
                    HtmlContent = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailMessage_EMailMessageTemplate_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "core",
                        principalTable: "EMailMessageTemplate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmailMessage_MultilingualCulture_CultureId",
                        column: x => x.CultureId,
                        principalSchema: "core",
                        principalTable: "MultilingualCulture",
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
                    ValidFrom = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    AuditAction = table.Column<int>(type: "integer", nullable: false),
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
                name: "AsyncTaskOperation",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationHandlerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationKey = table.Column<string>(type: "text", nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceType = table.Column<string>(type: "text", nullable: true),
                    ParameterSerialized = table.Column<string>(type: "text", nullable: true),
                    ExecuteById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: false),
                    ExecuteAt = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SortIndex = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRetryCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsyncTaskOperation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsyncTaskOperation_ApplicationUser_ExecuteById",
                        column: x => x.ExecuteById,
                        principalSchema: "identity",
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BinaryContent",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceType = table.Column<string>(type: "text", nullable: true),
                    MimeType = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<byte[]>(type: "bytea", nullable: true),
                    ContentSize = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
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
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceType = table.Column<string>(type: "text", nullable: true),
                    ChronologyType = table.Column<int>(type: "integer", nullable: true),
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
                    ValidFrom = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "TIMESTAMP WITHOUT TIME ZONE", nullable: true),
                    AuditAction = table.Column<int>(type: "integer", nullable: false),
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
                name: "ChronologyEntryTranslation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true),
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
                        principalSchema: "core",
                        principalTable: "MultilingualCulture",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "core",
                table: "MultilingualCulture",
                columns: new[] { "Id", "IsActive", "IsDefault", "Name" },
                values: new object[,]
                {
                    { new Guid("023eb000-7fdf-4ef5-aa76-bc116f59ebef"), true, true, "de" },
                    { new Guid("a0af7864-0dc1-49a6-a0d8-2f29157b3801"), true, false, "en" }
                });

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
                name: "IX_ApplicationUser_PreferedCultureId",
                schema: "identity",
                table: "ApplicationUser",
                column: "PreferedCultureId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "identity",
                table: "ApplicationUser",
                column: "NormalizedUserName",
                unique: true);

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
                name: "IX_AsyncTaskOperation_ExecuteAt_Status",
                schema: "core",
                table: "AsyncTaskOperation",
                columns: new[] { "ExecuteAt", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AsyncTaskOperation_ExecuteById",
                schema: "core",
                table: "AsyncTaskOperation",
                column: "ExecuteById");

            migrationBuilder.CreateIndex(
                name: "IX_AsyncTaskOperation_OperationKey",
                schema: "core",
                table: "AsyncTaskOperation",
                column: "OperationKey");

            migrationBuilder.CreateIndex(
                name: "IX_AsyncTaskOperation_ReferenceId_ReferenceType",
                schema: "core",
                table: "AsyncTaskOperation",
                columns: new[] { "ReferenceId", "ReferenceType" });

            migrationBuilder.CreateIndex(
                name: "IX_AsyncTaskOperation_ReferenceId_Status",
                schema: "core",
                table: "AsyncTaskOperation",
                columns: new[] { "ReferenceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundserviceInfo_Name",
                schema: "core",
                table: "BackgroundserviceInfo",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BinaryContent_CreatedById",
                schema: "core",
                table: "BinaryContent",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BinaryContent_ModifiedById",
                schema: "core",
                table: "BinaryContent",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_BinaryContent_ReferenceId_ReferenceType",
                schema: "core",
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
                name: "IX_ChronologyEntryTranslation_CoreId",
                table: "ChronologyEntryTranslation",
                column: "CoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ChronologyEntryTranslation_CoreId_CultureId",
                table: "ChronologyEntryTranslation",
                columns: new[] { "CoreId", "CultureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChronologyEntryTranslation_CultureId",
                table: "ChronologyEntryTranslation",
                column: "CultureId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessage_CultureId",
                schema: "core",
                table: "EmailMessage",
                column: "CultureId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessage_ReferenceId_ReferenceType",
                schema: "core",
                table: "EmailMessage",
                columns: new[] { "ReferenceId", "ReferenceType" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessage_Status_SendAt",
                schema: "core",
                table: "EmailMessage",
                columns: new[] { "Status", "SendAt" });

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

            migrationBuilder.CreateIndex(
                name: "IX_MultilingualCulture_IsDefault",
                schema: "core",
                table: "MultilingualCulture",
                column: "IsDefault",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MultilingualCulture_Name",
                schema: "core",
                table: "MultilingualCulture",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MultilingualGlobalText_CultureId_Key",
                schema: "core",
                table: "MultilingualGlobalText",
                columns: new[] { "CultureId", "Key" },
                unique: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationRoleClaim",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "ApplicationUserAudit",
                schema: "audit");

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
                name: "AsyncTaskOperation",
                schema: "core");

            migrationBuilder.DropTable(
                name: "BackgroundserviceInfo",
                schema: "core");

            migrationBuilder.DropTable(
                name: "ChronologyEntryTranslation");

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
                name: "MultilingualGlobalText",
                schema: "core");

            migrationBuilder.DropTable(
                name: "PersonAudit",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "ApplicationRole",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "ChronologyEntry");

            migrationBuilder.DropTable(
                name: "BinaryContent",
                schema: "core");

            migrationBuilder.DropTable(
                name: "EmailMessage",
                schema: "core");

            migrationBuilder.DropTable(
                name: "Person");

            migrationBuilder.DropTable(
                name: "ApplicationUser",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "EMailMessageTemplate",
                schema: "core");

            migrationBuilder.DropTable(
                name: "MultilingualCulture",
                schema: "core");

            migrationBuilder.DropTable(
                name: "EMailMessageConfiguration",
                schema: "core");
        }
    }
}
