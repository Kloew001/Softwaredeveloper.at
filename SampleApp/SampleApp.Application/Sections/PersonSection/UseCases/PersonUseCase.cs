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
) : UseCase<CreatePersonDto, Guid>
{
    private readonly PersonService _personService = personService;
    private readonly AccessConditionService _accessConditionService = accessConditionService;

    public override async ValueTask<bool> IsAvailableAsync()
    {
        return await _personService.CanCreateAsync() &&
            await _accessConditionService.IsAdminAsync();
    }

    protected override async Task<Guid> OnExecute(CreatePersonDto dto, CancellationToken cancellationToken)
    {
        var personId = await _personService.QuickCreateAsync(dto);

        return personId;
    }
}
