
namespace SampleApp.Application.Sections.PersonSection
{
    public class PersonService : EntityService<Person>
    {
        public PersonService(
            IDbContext context,
            AccessService accessService,
            SectionManager sectionManager,
            PersonValidator validator)
            : base(context, accessService, sectionManager, validator)
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
