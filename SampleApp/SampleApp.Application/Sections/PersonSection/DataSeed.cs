using Microsoft.Extensions.DependencyInjection;


namespace SampleApp.Application.Sections.PersonSection
{
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


        public Task SeedAsync()
        {
            return Task.CompletedTask;

            //if (_context.Set<Person>().Any())
            //    return;

            //for (int i = 0; i < 10; i++)
            //{
            //    using (var scope = _scopeFactory.CreateScope())
            //    {
            //        var context = scope.ServiceProvider.GetRequiredService<IDbContext>();
            //        var service = scope.ServiceProvider.GetRequiredService<PersonService>();

            //        //TODO

            //        await context.SaveChangesAsync();
            //    }
            //}
        }
    }
}
