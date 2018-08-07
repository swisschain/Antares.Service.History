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
#pragma warning disable 4014
            _executionQueueReader.Start();
#pragma warning restore 4014

            return Task.CompletedTask;
        }
    }
}
