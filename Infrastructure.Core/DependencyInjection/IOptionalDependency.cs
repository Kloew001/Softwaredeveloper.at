using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.DependencyInjection
{
    public interface IOptionalDependency<T>
    {
        T Value { get; }
    }

    public class OptionalDependency<T> : IOptionalDependency<T>, ITransientDependency
    {
        public OptionalDependency(IServiceProvider serviceProvider)
        {
            Value = serviceProvider.GetService<T>();
        }

        public T Value { get; }
    }
}
