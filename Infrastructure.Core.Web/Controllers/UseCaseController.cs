using Microsoft.AspNetCore.Mvc;

using SoftwaredeveloperDotAt.Infrastructure.Core.UseCases;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers;

public class UseCaseController(UseCaseService useCaseService) : BaseApiController
{
    private readonly UseCaseService _useCaseService = useCaseService;

    public class UseCaseEvaluateParamter
    {
        public IEnumerable<string> UseCaseIdentifiers { get; set; }
        public Dictionary<string, object> Parameter { get; set; }
    }

    [HttpPost]
    public ValueTask<IEnumerable<UseCaseService.UseCaseInfo>> Evaluate(UseCaseEvaluateParamter parameter)
        => _useCaseService.EvaluateAsync(parameter.UseCaseIdentifiers, parameter.Parameter);

    public class UseCaseExecuteParamter
    {
        public string UseCaseIdentifier { get; set; }
        public Dictionary<string, object> Parameter { get; set; }
    }

    [HttpPost]
    public Task<object> Execute(UseCaseExecuteParamter parameter)
        => _useCaseService.ExecuteAsync(parameter.UseCaseIdentifier, parameter.Parameter);

}