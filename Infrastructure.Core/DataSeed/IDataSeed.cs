namespace SoftwaredeveloperDotAt.Infrastructure.Core.DataSeed;

[ScopedDependency<IDataSeed>]
public interface IDataSeed
{
    decimal Priority { get; }
    bool AutoExecute { get; }

    Task SeedAsync(CancellationToken cancellationToken);
}