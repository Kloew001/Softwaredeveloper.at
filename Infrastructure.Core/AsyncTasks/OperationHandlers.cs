using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.AsyncTasks
{
    [OperationHandler(OperationHandlerId)]
    public class TestAsyncTaskOperationHandler : IAsyncTaskOperationHandler
    {
        public const string OperationHandlerId = "9837B2C0-2BF0-4B2C-AAFF-ECD41389DF16";
        public string OperationKey
            => $"{nameof(TestAsyncTaskOperationHandler)}#{TestId}#{TestProp1}#{TestProp2.Ticks}";

        public Guid? ReferenceId => TestId;

        public Guid TestId { get; set; }

        public string TestProp1 { get; set; }

        public DateTime TestProp2 { get; set; }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
