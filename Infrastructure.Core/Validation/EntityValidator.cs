using FluentValidation;

using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Validation
{
    public class EntityValidator<TEntity> : AbstractValidator<TEntity>, ITypedScopedDependency<EntityValidator<TEntity>>
        where TEntity : Entity
    {
    }
}
