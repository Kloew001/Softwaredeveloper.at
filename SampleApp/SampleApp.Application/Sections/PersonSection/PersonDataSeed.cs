using Microsoft.Extensions.DependencyInjection;

using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.DemoData;

namespace SampleApp.Application.Sections.PersonSection;

public class PersonDataSeed : IDataSeed
{
    public decimal Priority => 10;
    public bool ExecuteInThread { get; set; } = true;
    public bool AutoExecute { get; set; } = true;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDbContext _context;

    public PersonDataSeed(IServiceScopeFactory scopeFactory, IDbContext context)
    {
        _scopeFactory = scopeFactory;
        _context = context;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (_context.Set<Person>().Any())
            return;

        for (int i = 0; i < 10; i++)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IDbContext>();
                var service = scope.ServiceProvider.GetRequiredService<PersonService>();

                await service.CreateAsync(p =>
                {
                    p.FirstName = DemoDataHelper.GetRandomFirstName();
                    p.LastName = DemoDataHelper.GetRandomLastName();

                    //Random get DemoDataHelper.Adressen und create adress

                    p.Addresses = new List<Address>
                    {
                        new Address
                        {
                            IsMain = true,
                            Street = DemoDataHelper.Adressen[i].Street,
                            City = DemoDataHelper.Adressen[i].City,
                            ZipCode = DemoDataHelper.Adressen[i].ZipCode
                        },
                        new Address
                        {
                            IsMain = false,
                            Street = DemoDataHelper.Adressen[i+5].Street,
                            City = DemoDataHelper.Adressen[i+5].City,
                            ZipCode = DemoDataHelper.Adressen[i+5].ZipCode
                        }
                    };

                    return ValueTask.CompletedTask;
                });

                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}