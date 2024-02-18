using FluentValidation;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Validation
{
    public class EntityValidator<TEntity> : AbstractValidator<TEntity>, ITypedScopedService<EntityValidator<TEntity>>
        where TEntity : Entity
    {
    }
}
