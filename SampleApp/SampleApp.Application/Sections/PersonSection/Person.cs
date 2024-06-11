using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SoftwaredeveloperDotAt.Infrastructure.Core.Audit;

namespace SampleApp.Application.Sections.PersonSection
{
    [Table(nameof(Person))]
    public class Person : Entity, ISoftDelete, IAuditableEntity<PersonAudit>
    {
        public bool IsDeleted { get; set; } = false;

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public virtual ICollection<PersonAudit> Audits { get; set; }
    }

    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
        }
    }

    public class PersonAudit : EntityAudit<Person>
    {
        public bool IsDeleted { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
