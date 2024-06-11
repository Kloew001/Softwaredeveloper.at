using Microsoft.EntityFrameworkCore;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Audit
{
    public interface IAuditHandler : ITypedSingletonDependency<IAuditHandler>
    {
    }

}
