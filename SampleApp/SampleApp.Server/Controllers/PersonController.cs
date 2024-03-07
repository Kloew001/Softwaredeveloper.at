
using SampleApp.Application.Sections.PersonSection;

using SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers;

namespace RWA.Server.Controllers
{
    public class PersonController : BaseApiController
    {
        private readonly PersonService _service;

        public PersonController(PersonService service)
        {
            _service = service;
        }

        [HttpGet]
        public Task<IEnumerable<PersonDto>> GetCollectionAsync()
            => _service.GetCollectionAsync<PersonDto>();

        [HttpGet]
        public Task<PersonDto> GetSingleById(Guid id)
            => _service.GetSingleByIdAsync<PersonDto>(id);

        [HttpPost]
        public Task Validate(PersonDto dto)
            => _service.ValidateAndThrowAsync(dto);

        [HttpPost]
        public Task<Guid> Create(PersonDto dto)
             => _service.CreateAsync(dto);

        [HttpPost]
        public Task<Guid> QuickCreate()
             => _service.QuickCreateAsync(new PersonDto());
        
        [HttpPost]
        public Task<PersonDto> Update(PersonDto dto)
            => _service.UpdateAsync(dto);
    }
}
