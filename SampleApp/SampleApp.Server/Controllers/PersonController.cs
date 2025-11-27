
using SampleApp.Application.Sections.PersonSection;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers;

namespace SoftwaredeveloperDotAt.Server.Controllers;

public class PersonController : BaseApiController
{
    private readonly PersonService _service;

    public PersonController(PersonService service)
    {
        _service = service;
    }

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
         => (await _service.CreateAsync(dto)).Id.Value;

    [HttpPost]
    public Task<Guid> QuickCreate()
         => _service.QuickCreateAsync(new PersonDto());

    [HttpPost]
    public Task<PersonDto> Update(PersonDto dto)
        => _service.UpdateAsync(dto);
}
