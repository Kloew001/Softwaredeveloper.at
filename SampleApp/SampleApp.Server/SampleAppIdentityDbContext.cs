using Microsoft.EntityFrameworkCore;

using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Identity;

public class SampleAppIdentityDbContext : IdentitySoftwaredeveloperDotAtDbContext
{
    public SampleAppIdentityDbContext()
    {
    }

    public SampleAppIdentityDbContext(DbContextOptions<SampleAppIdentityDbContext> options)
        : base(options)
    {
    }
}
