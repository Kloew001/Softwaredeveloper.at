
namespace SampleApp.Application.Sections.PersonSection
{
    public class PersonService : EntityService<Person>
    {
        public PersonService(EntityServiceDependency<Person> entityServiceDependency) 
            : base(entityServiceDependency)
        {
        }

        protected override async Task OnCreateInternalAsync(Person person)
        {
            await base.OnCreateInternalAsync(person);

            if (person.FirstName.IsNullOrEmpty())
                person.FirstName = "John";
        }
    }
}
