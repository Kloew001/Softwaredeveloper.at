namespace SoftwaredeveloperDotAt.Infrastructure.Core.DataSeed
{
    public interface IDataSeed : ITypedScopedDependency<IDataSeed>
    {
        decimal Priority { get; }
        bool AutoExecute { get; set; }

        Task SeedAsync(CancellationToken cancellationToken);
    }
}