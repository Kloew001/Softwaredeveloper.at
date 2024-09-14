using DocumentFormat.OpenXml.InkML;

using Infrastructure.Core.Tests;

using Microsoft.Extensions.DependencyInjection;

using SampleApp.Application.Sections.PersonSection;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.UnitOfWork;

namespace SampleApp.Application.Tests
{
    public class UnitOfWorkTests : BaseTest<DomainStartup>
    {
        [Test]
        public async Task Test()
        {
            var unitOfWork = _serviceScope.ServiceProvider
                .GetService<IUnitOfWork>();

            var personService = _serviceScope.ServiceProvider
                .GetService<PersonService>();

            var workScopeId = personService.WorkScope.Id;

            using (var scope = unitOfWork.CreateScope())
            {
                var workScope2Id = personService.WorkScope.Id;
                Assert.That(workScopeId != workScope2Id);
            }

            Assert.That(workScopeId == personService.WorkScope.Id);

            var scope3 = unitOfWork.CreateScope();
            var workScope3Id = personService.WorkScope.Id;
            Assert.That(workScopeId != workScope3Id);

            var scope4 = unitOfWork.CreateScope();
            var workScope4Id = personService.WorkScope.Id;
            Assert.That(workScopeId != workScope4Id);

            scope4.Dispose();
            Assert.That(personService.WorkScope.Id == workScope3Id);
            
            scope3.Dispose();
            Assert.That(personService.WorkScope.Id == workScopeId);
        }
    }
}