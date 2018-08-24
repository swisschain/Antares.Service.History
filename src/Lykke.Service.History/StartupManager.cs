using System.Threading.Tasks;
using Lykke.Sdk;
using Lykke.Service.History.Workflow.ExecutionProcessing;

namespace Lykke.Service.History
{
    public class StartupManager : IStartupManager
    {
        private readonly ExecutionQueueReader _executionQueueReader;
        private readonly OrderEventQueueReader _orderEventQueueReader;

        public StartupManager(ExecutionQueueReader executionQueueReader, OrderEventQueueReader orderEventQueueReader)
        {
            _executionQueueReader = executionQueueReader;
            _orderEventQueueReader = orderEventQueueReader;
        }

        public Task StartAsync()
        {
            _executionQueueReader.Start();

            _orderEventQueueReader.Start();

            return Task.CompletedTask;
        }
    }
}
