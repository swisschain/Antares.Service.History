using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;

namespace Lykke.Service.History.Workflow.ExecutionProcessing
{
    public class ExecutionBatchHandlingStrategy 
    {
        private readonly int _batchSize;
        private readonly ILog _log;

        private int _currentCount;

        public ExecutionBatchHandlingStrategy(ILogFactory logFactory, int batchSize = 10)
        {
            _currentCount = 0;
            _batchSize = batchSize;
            _log = logFactory.CreateLog(this);
        }

        public void Execute(Action handler, MessageAcceptor ma, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _currentCount);

            Task.Run(handler, cancellationToken).ContinueWith(x =>
            {
                Interlocked.Decrement(ref _currentCount);

                if (x.IsCompleted)
                {
                    ma.Accept();
                }
                else
                {
                    _log.Error(x.Exception, "message was rejected");

                    ma.Reject();
                }
            }, cancellationToken);

            while (_currentCount > _batchSize)
            {
                Task.Delay(50, cancellationToken).GetAwaiter().GetResult();
            }
        }
    }
}
