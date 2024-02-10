﻿using System.ComponentModel;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using SoftwaredeveloperDotAt.Infrastructure.Core.AsyncTasks;
using SoftwaredeveloperDotAt.Infrastructure.Core.BackgroundServices;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.BinaryContent;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.ChronologyEntries;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.EMailMessage;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Multilingual;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework
{
    public abstract class BaseDbContext : DbContext,
        IDbContext,
        ITypedScopedService<IDbContext>

    {
        protected BaseDbContext()
        {
        }

        protected BaseDbContext(DbContextOptions options) : base(options)
        {
        }

        //protected IServiceProvider ServiceProvider { get; }

        //public BaseDbContext(IServiceProvider serviceProvider)
        //{
        //    ServiceProvider = serviceProvider;
        //}

        //public BaseDbContext(DbContextOptions options, IServiceProvider serviceProvider)
        //    : base(options)
        //{
        //    ServiceProvider = serviceProvider;
        //}

        public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public virtual DbSet<ApplicationUserClaim> ApplicationUserClaims { get; set; }

        public virtual DbSet<ApplicationUserLogin> ApplicationUserLogins { get; set; }

        public virtual DbSet<ApplicationUserToken> ApplicationUserTokens { get; set; }

        public virtual DbSet<ApplicationRole> ApplicationRoles { get; set; }

        public virtual DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }

        public virtual DbSet<ApplicationRoleClaim> ApplicationRoleClaims { get; set; }

        public virtual DbSet<ChronologyEntry> ChronologyEntries { get; set; }

        public virtual DbSet<BinaryContent> Contents { get; set; }

        public virtual DbSet<EmailMessage> EmailMessages { get; set; }
        public virtual DbSet<AsyncTaskOperation> AsyncTaskOperations { get; set; }
        public virtual DbSet<BackgroundserviceInfo> BackgroundserviceInfos { get; set; }

        //public virtual DbSet<LanguageCulture> LanguageCultures { get; set; }
        //public virtual DbSet<MultilingualText> Multilinguals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var dbContextHandler = this.GetService<IDbContextHandler>();
            dbContextHandler.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            BeforeSaveChanges();

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            BeforeSaveChanges();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void BeforeSaveChanges()
        {
            var dbContextHandler = this.GetService<IDbContextHandler>();

            if (dbContextHandler == null)
                throw new Exception($"Could not resolve {nameof(IDbContextHandler)}");

            dbContextHandler.UpdateChangeTrackedEntity(this);
        }

        TEntity IDbContext.CreateProxy<TEntity>(params object[] constructorArguments) where TEntity : class
        {
            return ProxiesExtensions.CreateProxy<TEntity>(this, constructorArguments);
        }
    }
}