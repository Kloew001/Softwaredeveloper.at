using Microsoft.EntityFrameworkCore;
using SampleApp.Application.Sections.PersonSection;

namespace SampleApp.Application;

public class SampleAppContext : SoftwaredeveloperDotAtDbContext
{
    public SampleAppContext()
    {
    }

    public SampleAppContext(DbContextOptions<SampleAppContext> options)
        : base(options)
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