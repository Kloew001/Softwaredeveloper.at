using Microsoft.EntityFrameworkCore;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SampleApp.Application.Sections.PersonSection;

namespace SampleApp.Application
{
    public class SampleAppContext : BaseSoftwaredeveloperDotAtDbContext
    {
        public SampleAppContext()
        {
        }

        public SampleAppContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SampleAppContext).Assembly);
        }

        public virtual DbSet<Person> Persons { get; set; }
        
    }
}
