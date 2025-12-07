using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers;

[ApiController]
[Authorize(policy: "api")]
[Route("api/[controller]/[action]")]
public abstract class BaseApiController : ControllerBase
{
}