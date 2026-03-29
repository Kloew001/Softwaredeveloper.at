
using SampleApp.Application.Sections.PersonSection;
using SampleApp.Application.Sections.PersonSection.UseCases;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.UseCases;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers;

namespace SampleApp.Server.Controllers;

[AllowAnonymous]
public class PersonController(
    UseCaseService useCaseService,
    PersonService service
) : BaseApiController
{
    private readonly PersonService _service = service;
    private readonly UseCaseService _useCaseService = useCaseService;

    [HttpGet]
    public Task<IEnumerable<PersonDto>> GetAll()
        => _service.GetAllAsync();

    [HttpGet]
    public Task<PageResult<PersonDto>> GetOverview([FromQuery] PersonService.PersonOverviewFilter filter)
        => _service.GetOverviewAsync(filter);

    [HttpGet]
    public Task<PersonDto> GetSingleById(Guid id)
        => _service.GetSingleByIdAsync<PersonDto>(id);

    [HttpPost]
    public Task Validate(PersonDto dto)
        => _service.ValidateAndThrowAsync(dto);

    [HttpPost]
    public async Task<Guid> Create(PersonDto dto)
         => await _service.CreateAsync(dto);

    [HttpPost("quickcreate")]
    public async Task<Guid> QuickCreate(CancellationToken cancellationToken)
    {
        var parameter = new CreatePersonDto() { };
        return await _useCaseService.ExecuteAsync<QuickCreatePersonUseCase, Guid, CreatePersonDto>(parameter, cancellationToken);
    }

    [HttpPost]
    public Task Update(PersonDto dto)
        => _service.UpdateAsync(dto);

    [HttpPost]
    public Task Delte(Guid id)
        => _service.DeleteAsync(id);

}