using Microsoft.AspNetCore.Mvc;

using SoftwaredeveloperDotAt.Infrastructure.Core.UseCases;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers
{
    public class UseCaseController : BaseApiController
    {
        private readonly UseCaseService _useCaseService;

        public UseCaseController(UseCaseService useCaseService)
        {
            _useCaseService = useCaseService;
        }

        public class UseCaseParamter
        {
            public IEnumerable<Guid> UseCaseIds { get; set; }
            public Guid? UseCaseId { get; set; }
            public Dictionary<string, object> Parameter { get; set; }
        }

        [HttpPost]
        public Task<IEnumerable<UseCaseService.UseCaseInfo>> Evaluate(UseCaseParamter parameter)
            => _useCaseService.EvaluateAsync(parameter.UseCaseIds, parameter.Parameter);
        
        [HttpPost]
        public Task Execute(UseCaseParamter parameter)
            => _useCaseService.ExecuteAsync(parameter.UseCaseId.Value, parameter.Parameter);
    }
}
