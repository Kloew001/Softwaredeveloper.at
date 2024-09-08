using Microsoft.Extensions.DependencyInjection;

using SampleApp.Application.Sections.PersonSection;

using SoftwaredeveloperDotAt.Infrastructure.Core;

namespace SampleApp.Application.Tests
{
    public class PersonTests : SampleAppBaseTest
    {
        [Test]
        public async Task CreatePerson()
        {
            var personService = 
                _serviceScope.ServiceProvider
                .GetService<PersonService>();

            var persons = await personService.GetAllAsync();
            var countBefore = persons.Count();

            var personId = await personService.CreateAsync(new PersonDto
            {
                FirstName = "John",
                LastName = "Doe"
            });

            persons = await personService.GetAllAsync();

            Assert.That(persons.Count() == countBefore + 1);
            Assert.That(persons.Any(_ => _.Id == personId.Id));
        }
    }
}