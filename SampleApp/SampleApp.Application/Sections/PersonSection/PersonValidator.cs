using FluentValidation;

namespace SampleApp.Application.Sections.PersonSection;

public class PersonValidator : EntityValidator<Person>
{
    public PersonValidator(EntityValidatorDependency<Person> dependency)
       : base(dependency)
    {
        RuleFor(_ => _.FirstName)
            .NotNull();

        RuleFor(_ => _.LastName)
            .NotNull();
    }
}