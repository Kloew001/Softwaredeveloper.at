using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Controllers
{
    public class MonitoreController : BaseApiController
    {
        private readonly IDbContext _dbContext;
        public MonitoreController(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [AllowAnonymous]
        public bool IsAlive() => true;

        [HttpGet]
        [AllowAnonymous]
        public bool IsDBConnected()
        {
            _dbContext.Database.OpenConnection();
            return true;
        }
    }
}
