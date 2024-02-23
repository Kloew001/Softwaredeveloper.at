﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SampleApp.Application;

#nullable disable

namespace SampleApp.Application.Migrations
{
    [DbContext(typeof(SampleAppContext))]
    partial class SampleAppContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SampleApp.Application.Sections.PersonSection.Person", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("FirstName")
                        .HasColumnType("text");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("LastName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Person");

                    b.HasData(
                        new
                        {
                            Id = new Guid("144fcd4a-3b46-4ff4-ad82-734d037f3e2d"),
                            FirstName = "Huber",
                            IsDeleted = false,
                            LastName = "Tester"
                        });
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.AsyncTasks.AsyncTaskOperation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TIMESTAMP WITHOUT TIME ZONE");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<DateTime?>("ExecuteAt")
                        .HasColumnType("TIMESTAMP WITHOUT TIME ZONE");

                    b.Property<Guid?>("ExecuteById")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("FinishedAt")
                        .HasColumnType("TIMESTAMP WITHOUT TIME ZONE");

                    b.Property<Guid>("OperationHandlerId")
                        .HasColumnType("uuid");

                    b.Property<string>("OperationKey")
                        .HasColumnType("text");

                    b.Property<string>("ParameterSerialized")
                        .HasColumnType("text");

                    b.Property<Guid?>("ReferenceId")
                        .HasColumnType("uuid");

                    b.Property<int>("RetryCount")
                        .HasColumnType("integer");

                    b.Property<int>("SortIndex")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("StartedAt")
                        .HasColumnType("TIMESTAMP WITHOUT TIME ZONE");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ExecuteById");

                    b.HasIndex("ExecuteAt", "Status");

                    b.ToTable("AsyncTaskOperations");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.BackgroundServices.BackgroundserviceInfo", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("ExecutedAt")
                        .HasColumnType("TIMESTAMP WITHOUT TIME ZONE");

                    b.Property<DateTime?>("LastErrorAt")
                        .HasColumnType("TIMESTAMP WITHOUT TIME ZONE");

                    b.Property<string>("LastErrorMessage")
                        .HasColumnType("text");

                    b.Property<string>("LastErrorStack")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastFinishedAt")
                        .HasColumnType("TIMESTAMP WITHOUT TIME ZONE");

                    b.Property<string>("Message")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<DateTime?>("NextExecuteAt")
                        .HasColumnType("TIMESTAMP WITHOUT TIME ZONE");

                    b.HasKey("Id");

                    b.ToTable("BackgroundserviceInfo");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationRole", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("ConcurrencyStamp")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "NormalizedName" }, "RoleNameIndex")
                        .IsUnique();

                    b.ToTable("ApplicationRole", "identity");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationRoleClaim", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "RoleId" }, "IX_ApplicationRoleClaim_RoleId");

                    b.ToTable("ApplicationRoleClaim", "identity");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUser", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .HasColumnType("text");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("TIMESTAMP WITHOUT TIME ZONE");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("FirstName")
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .HasColumnType("text");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LockoutEnd")
                        .HasColumnType("TIMESTAMP WITHOUT TIME ZONE");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "NormalizedEmail" }, "EmailIndex");

                    b.HasIndex(new[] { "NormalizedUserName" }, "UserNameIndex")
                        .IsUnique();

                    b.ToTable("ApplicationUser", "identity");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUserClaim", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "UserId" }, "IX_ApplicationUserClaim_UserId");

                    b.ToTable("ApplicationUserClaim", "identity");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUserLogin", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex(new[] { "UserId" }, "IX_ApplicationUserLogin_UserId");

                    b.ToTable("ApplicationUserLogin", "identity");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUserRole", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex(new[] { "RoleId" }, "IX_ApplicationUserRole_RoleId");

                    b.ToTable("ApplicationUserRole", "identity");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUserToken", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("ApplicationUserToken", "identity");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.Multilingual.LanguageCulture", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("IsDefault");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("LanguageCulture");

                    b.HasData(
                        new
                        {
                            Id = new Guid("023eb000-7fdf-4ef5-aa76-bc116f59ebef"),
                            IsDefault = false,
                            IsEnabled = false,
                            Name = "de-AT"
                        },
                        new
                        {
                            Id = new Guid("a0af7864-0dc1-49a6-a0d8-2f29157b3801"),
                            IsDefault = false,
                            IsEnabled = false,
                            Name = "en-US"
                        });
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.Sections.BinaryContent.BinaryContent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<byte[]>("Content")
                        .HasColumnType("bytea");

                    b.Property<long>("ContentSize")
                        .HasColumnType("bigint");

                    b.Property<Guid>("CreatedById")
                        .HasColumnType("uuid")
                        .HasColumnName("CreatedById");

                    b.Property<DateTime>("DateCreated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TIMESTAMP WITHOUT TIME ZONE")
                        .HasDefaultValueSql("NOW()");

                    b.Property<DateTime>("DateModified")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TIMESTAMP WITHOUT TIME ZONE")
                        .HasDefaultValueSql("NOW()");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("MimeType")
                        .HasColumnType("text");

                    b.Property<Guid>("ModifiedById")
                        .HasColumnType("uuid")
                        .HasColumnName("ModifiedById");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<Guid?>("ReferenceId")
                        .HasColumnType("uuid");

                    b.Property<string>("ReferenceType")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("ModifiedById");

                    b.HasIndex("ReferenceId", "ReferenceType");

                    b.ToTable("BinaryContent");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChronologyEntries.ChronologyEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CreatedById")
                        .HasColumnType("uuid")
                        .HasColumnName("CreatedById");

                    b.Property<DateTime>("DateCreated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TIMESTAMP WITHOUT TIME ZONE")
                        .HasDefaultValueSql("NOW()");

                    b.Property<DateTime>("DateModified")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TIMESTAMP WITHOUT TIME ZONE")
                        .HasDefaultValueSql("NOW()");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<Guid>("ModifiedById")
                        .HasColumnType("uuid")
                        .HasColumnName("ModifiedById");

                    b.Property<Guid?>("ReferenceId")
                        .HasColumnType("uuid");

                    b.Property<string>("ReferenceType")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("ModifiedById");

                    b.HasIndex("ReferenceId", "ReferenceType");

                    b.ToTable("ChronologyEntry");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EMailMessage.EmailMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AnAdress")
                        .HasColumnType("text");

                    b.Property<byte[]>("Attachment1")
                        .HasColumnType("bytea");

                    b.Property<string>("Attachment1Name")
                        .HasColumnType("text");

                    b.Property<byte[]>("Attachment2")
                        .HasColumnType("bytea");

                    b.Property<string>("Attachment2Name")
                        .HasColumnType("text");

                    b.Property<string>("BccAdress")
                        .HasColumnType("text");

                    b.Property<string>("CcAdress")
                        .HasColumnType("text");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("TIMESTAMP WITHOUT TIME ZONE");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<string>("HtmlContent")
                        .HasColumnType("text");

                    b.Property<Guid?>("ReferenceId")
                        .HasColumnType("uuid");

                    b.Property<string>("ReferenceType")
                        .HasColumnType("text");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Subject")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("EmailMessage");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.AsyncTasks.AsyncTaskOperation", b =>
                {
                    b.HasOne("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUser", "ExecuteBy")
                        .WithMany()
                        .HasForeignKey("ExecuteById");

                    b.Navigation("ExecuteBy");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationRoleClaim", b =>
                {
                    b.HasOne("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationRole", "Role")
                        .WithMany("ApplicationRoleClaims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUserClaim", b =>
                {
                    b.HasOne("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUser", "User")
                        .WithMany("ApplicationUserClaims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUserLogin", b =>
                {
                    b.HasOne("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUser", "User")
                        .WithMany("ApplicationUserLogins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUserRole", b =>
                {
                    b.HasOne("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationRole", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUser", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUserToken", b =>
                {
                    b.HasOne("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUser", "User")
                        .WithMany("ApplicationUserTokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.Sections.BinaryContent.BinaryContent", b =>
                {
                    b.HasOne("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUser", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUser", "ModifiedBy")
                        .WithMany()
                        .HasForeignKey("ModifiedById")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("CreatedBy");

                    b.Navigation("ModifiedBy");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChronologyEntries.ChronologyEntry", b =>
                {
                    b.HasOne("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUser", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUser", "ModifiedBy")
                        .WithMany()
                        .HasForeignKey("ModifiedById")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("CreatedBy");

                    b.Navigation("ModifiedBy");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationRole", b =>
                {
                    b.Navigation("ApplicationRoleClaims");

                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework.ApplicationUser", b =>
                {
                    b.Navigation("ApplicationUserClaims");

                    b.Navigation("ApplicationUserLogins");

                    b.Navigation("ApplicationUserTokens");

                    b.Navigation("UserRoles");
                });
#pragma warning restore 612, 618
        }
    }
}
