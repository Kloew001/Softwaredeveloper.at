using System;

namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public interface IDataSeed : IScopedService
    {
        int Priority { get; }
        bool ExecuteInThread { get; set; }

        Task SeedAsync();
    }
}
