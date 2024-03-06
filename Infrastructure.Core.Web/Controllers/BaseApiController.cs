using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers
{
    [ApiController]
    [Authorize(policy: "api")]
    [Route("api/[controller]/[Action]")]
    public abstract class BaseApiController : Controller
    {
    }
}
