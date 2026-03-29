using SoftwaredeveloperDotAt.Infrastructure.Core.UseCases;

namespace SampleApp.Application.Sections.PersonSection.UseCases;

public class CreatePersonDto : Dto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class QuickCreatePersonUseCase(
    PersonService personService,
    AccessConditionService accessConditionService
) : QuickCreateUseCase<Person, CreatePersonDto>(personService)
{
    private readonly PersonService _personService = personService;
    private readonly AccessConditionService _accessConditionService = accessConditionService;
    
    public override async ValueTask<bool> IsAvailableAsync()
    {
        if (await base.IsAvailableAsync() == false)
            return false;

        return await _accessConditionService.IsAdminAsync();
    }
}
