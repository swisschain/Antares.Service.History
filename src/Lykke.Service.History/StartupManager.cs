using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Sdk;
using Lykke.Service.History.Workflow.ExecutionProcessing;

namespace Lykke.Service.History
{
    public class StartupManager : IStartupManager
    {
        private readonly ExecutionQueueReader _executionQueueReader;

        public StartupManager(ExecutionQueueReader executionQueueReader)
        {
            _executionQueueReader = executionQueueReader;
        }

        public Task StartAsync()
        {
            _executionQueueReader.Start();

            return Task.CompletedTask;
        }
    }
}
