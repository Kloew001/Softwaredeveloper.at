using Microsoft.Extensions.DependencyInjection;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.DependencyInjection;

public interface IOptionalDependency<T>
{
    T Value { get; }
}

[TransientDependency]
public class OptionalDependency<T> : IOptionalDependency<T>
{
    public OptionalDependency(IServiceProvider serviceProvider)
    {
        Value = serviceProvider.GetService<T>();
    }

    public T Value { get; }
}