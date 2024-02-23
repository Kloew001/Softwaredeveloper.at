using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SampleApp.Application.Sections.PersonSection
{

    [Table(nameof(Person))]
    public class Person : Entity, ISoftDelete
    {
        public bool IsDeleted { get; set; } = false;

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.HasData(new Person()
            {
                Id = new Guid("144FCD4A-3B46-4FF4-AD82-734D037F3E2D"),
                FirstName = "Huber",
                LastName = "Tester"
            });
        }
    }
}
