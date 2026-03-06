using SampleApp.Application.Sections.CarSection;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;
using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers;

namespace SampleApp.Server.Controllers;

[AllowAnonymous]
public class CarController(CarService service) : BaseApiController
{
    private readonly CarService _service = service;

    [HttpGet]
    public Task<IEnumerable<CarOverviewDto>> GetOverview()
        => _service.GetCollectionAsync<CarOverviewDto>();

    [HttpGet]
    public Task<IEnumerable<CarDto>> GetAll()
        => _service.GetCollectionAsync<CarDto>();

    [HttpGet]
    public Task<IEnumerable<ElectricCarDto>> GetAllElectricCar()
        => _service.GetCollectionAsync<ElectricCarDto>(q => q.OfType<ElectricCar>());

    [HttpGet]
    public Task<CarOverviewDto> GetSingleById(Guid id)
        => _service.GetSingleByIdAsync<CarOverviewDto>(id);

    [HttpPost]
    public Task Validate(CarDto dto)
        => _service.ValidateAndThrowAsync(dto);

    [HttpPost]
    public async Task<Guid> Create(CarDto dto)
         => (await _service.CreateAsync(dto)).Id.Value;

    [HttpPost]
    public Task<CarDto> Update(CarDto dto)
        => _service.UpdateAsync(dto);
}
