using System.Threading.Tasks;
using Antares.Job.History.Workflow.ExecutionProcessing;
using Antares.Sdk.Services;
using Lykke.Cqrs;

namespace Antares.Job.History
{
    public class StartupManager : IStartupManager
    {
        private readonly ICqrsEngine _cqrsEngine;
        private readonly ExecutionQueueReader _executionQueueReader;
        private readonly OrderEventQueueReader _orderEventQueueReader;

        public StartupManager(
            ICqrsEngine cqrsEngine,
            ExecutionQueueReader executionQueueReader,
            OrderEventQueueReader orderEventQueueReader
            )
        {
            _cqrsEngine = cqrsEngine;
            _executionQueueReader = executionQueueReader;
            _orderEventQueueReader = orderEventQueueReader;
        }

        public Task StartAsync()
        {
            _executionQueueReader.Start();
            _orderEventQueueReader.Start();
            _cqrsEngine.StartSubscribers();

            return Task.CompletedTask;
        }
    }
}
