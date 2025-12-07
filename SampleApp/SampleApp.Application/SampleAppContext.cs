using Microsoft.EntityFrameworkCore;

using SampleApp.Application.Sections.PersonSection;

namespace SampleApp.Application;

public class SampleAppContext : SoftwaredeveloperDotAtDbContext
{
    public SampleAppContext(DbContextOptions<SampleAppContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Person> Persons { get; set; }
}