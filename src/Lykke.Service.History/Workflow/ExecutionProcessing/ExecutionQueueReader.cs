using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
using MoreLinq;

namespace Lykke.Service.History.Workflow.ExecutionProcessing
{
    public class ExecutionQueueReader : IDisposable
    {
        private const int ExecutionsBulkSize = 30;
        private const int TradesBulkSize = 5000;

        private readonly ILogFactory _logFactory;
        private readonly string _connectionString;
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly IOrdersRepository _ordersRepository;

        private ConcurrentQueue<KeyValuePair<List<Order>, TaskCompletionSource<bool>>> _queue;

        private RabbitMqSubscriber<ExecutionProcessedEvent> _subscriber;
        private Task _saveTask;
        private CancellationTokenSource _cancellationTokenSource;


        public ExecutionQueueReader(
            ILogFactory logFactory,
            string connectionString,
            IHistoryRecordsRepository historyRecordsRepository,
            IOrdersRepository ordersRepository)
        {
            _logFactory = logFactory;
            _connectionString = connectionString;
            _historyRecordsRepository = historyRecordsRepository;
            _ordersRepository = ordersRepository;
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _queue = new ConcurrentQueue<KeyValuePair<List<Order>, TaskCompletionSource<bool>>>();

            var settings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = _connectionString,
                QueueName = "lykke.history.queue.post-processing.events.reader.projections",
                ExchangeName = "lykke.post-processing.events.exchange",
                RoutingKey = "ExecutionProcessedEvent",
                IsDurable = true
            };

            _subscriber = new RabbitMqSubscriber<ExecutionProcessedEvent>(
                    _logFactory,
                    settings,
                    new ExecutionBatchHandlingStrategy(_logFactory, ExecutionsBulkSize))
                .SetMessageDeserializer(new ProtobufMessageDeserializer<ExecutionProcessedEvent>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessage)
                .CreateDefaultBinding()
                .Start();

            _saveTask = StartSaving();
        }

        public void Stop()
        {
            _subscriber?.Stop();
            _cancellationTokenSource.Cancel();
        }

        private async Task<bool> ProcessMessage(ExecutionProcessedEvent message)
        {
            var tcs = new TaskCompletionSource<bool>();

            _queue.Enqueue(KeyValuePair.Create(Mapper.Map<IEnumerable<Order>>(message).ToList(), tcs));

            Console.WriteLine(_queue.Count);

            return await tcs.Task;
        }

        private async Task StartSaving()
        {
            while (!_cancellationTokenSource.IsCancellationRequested || _queue.Count > 0)
            {
                var list = new List<KeyValuePair<List<Order>, TaskCompletionSource<bool>>>();

                try
                {
                    for (var i = 0; i < ExecutionsBulkSize; i++)
                    { 
                        if (_queue.TryDequeue(out var keyValuePair))
                            list.Add(keyValuePair);
                        else
                            break;
                    }

                    if (list.Count > 0)
                    {
                        var orders = list.SelectMany(x => x.Key).ToList();

                        await _ordersRepository.UpsertBulkAsync(orders);

                        var trades = orders.SelectMany(x => x.Trades);

                        var batched = trades.Batch(TradesBulkSize).ToList();

                        foreach (var tradesBatch in batched)
                        {
                            await _historyRecordsRepository.TryInsertBulkAsync(tradesBatch);
                        }
                    }
                }
                finally
                {
                    foreach (var item in list)
                        item.Value.SetResult(true);
                }

                await Task.Delay(100);
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
