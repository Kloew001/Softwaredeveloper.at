using Microsoft.AspNetCore.Mvc;

using SoftwaredeveloperDotAt.Infrastructure.Core.UseCases;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers;

public class UseCaseController : BaseApiController
{
    private readonly UseCaseService _useCaseService;

    public UseCaseController(UseCaseService useCaseService)
    {
        _useCaseService = useCaseService;
    }

    public class UseCaseParamter
    {
        public IEnumerable<string> UseCaseIdentifiers { get; set; }
        public string UseCaseIdentifier { get; set; }
        public Dictionary<string, object> Parameter { get; set; }
    }

    [HttpPost]
    public ValueTask<IEnumerable<UseCaseService.UseCaseInfo>> Evaluate(UseCaseParamter parameter)
        => _useCaseService.EvaluateAsync(parameter.UseCaseIdentifiers, parameter.Parameter);
    
    [HttpPost]
    public Task<object> Execute(UseCaseParamter parameter)
        => _useCaseService.ExecuteAsync(parameter.UseCaseIdentifier, parameter.Parameter);
}
