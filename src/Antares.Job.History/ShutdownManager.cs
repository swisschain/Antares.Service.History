using System.Threading.Tasks;
using Antares.Job.History.Workflow.ExecutionProcessing;
using Antares.Sdk.Services;

namespace Antares.Job.History
{
    public class ShutdownManager : IShutdownManager
    {
        private readonly ExecutionQueueReader _executionQueueReader;
        private readonly OrderEventQueueReader _orderEventQueueReader;

        public ShutdownManager(
            ExecutionQueueReader executionQueueReader,
            OrderEventQueueReader orderEventQueueReader
            )
        {
            _executionQueueReader = executionQueueReader;
            _orderEventQueueReader = orderEventQueueReader;
        }

        public Task StopAsync()
        {
            _executionQueueReader.Stop();
            _orderEventQueueReader.Stop();

            return Task.CompletedTask;
        }
    }
}
