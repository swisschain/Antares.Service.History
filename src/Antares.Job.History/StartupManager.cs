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

        public StartupManager(
            ICqrsEngine cqrsEngine,
            ExecutionQueueReader executionQueueReader)
        {
            _cqrsEngine = cqrsEngine;
            _executionQueueReader = executionQueueReader;
        }

        public Task StartAsync()
        {
            _executionQueueReader.Start();
            _cqrsEngine.StartSubscribers();

            return Task.CompletedTask;
        }
    }
}
