using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.History.PostgresRepositories;
using Lykke.Service.PostProcessing.Contracts.Cqrs;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
using MoreLinq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ConnectionFactory = RabbitMQ.Client.ConnectionFactory;

namespace Lykke.Service.History.Workflow.ExecutionProcessing
{
    public class ExecutionQueueReader : IDisposable
    {
        private const int ExecutionsBulkSize = 100;
        private const int TradesBulkSize = 5000;

        private readonly string _connectionString;
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly IOrdersRepository _ordersRepository;
        private readonly ILog _log;
        private readonly IHealthNotifier _healthNotifier;

        private ConcurrentQueue<CustomQueueItem<IEnumerable<Order>>> _queue;
        private Task _dbWriterTask;
        private Task _queueReaderTask;
        private CancellationTokenSource _cancellationTokenSource;
        

        public ExecutionQueueReader(
            ILogFactory logFactory,
            string connectionString,
            IHistoryRecordsRepository historyRecordsRepository,
            IOrdersRepository ordersRepository, IHealthNotifier healthNotifier)
        {
            _connectionString = connectionString;
            _historyRecordsRepository = historyRecordsRepository;
            _ordersRepository = ordersRepository;
            _healthNotifier = healthNotifier;
            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _queue = new ConcurrentQueue<CustomQueueItem<IEnumerable<Order>>>();
            _dbWriterTask = StartDbWriter();
            _queueReaderTask = StartQueueReader().ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    _healthNotifier.Notify("Queue reader unexpectedly stopped");
                    _log.Error(x.Exception);
                }
            });
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();

            Task.WaitAny(_queueReaderTask, Task.Delay(10000));
        }

        private async Task StartQueueReader()
        {
            var exchangeName = $"lykke.{PostProcessingBoundedContext.Name}.events.exchange";
            var queueName = $"lykke.history.queue.{PostProcessingBoundedContext.Name}.events.custom-reader";

            var factory = new ConnectionFactory
            {
                Uri = _connectionString
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.BasicQos(0, ExecutionsBulkSize * 2, false);

                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

                channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: nameof(ExecutionProcessedEvent));

                var consumer = new EventingBasicConsumer(channel);

                var deserializer = new ProtobufMessageDeserializer<ExecutionProcessedEvent>();

                consumer.Received += (o, basicDeliverEventArgs) =>
                {
                    try
                    {
                        var message = deserializer.Deserialize(basicDeliverEventArgs.Body);

                        _queue.Enqueue(new CustomQueueItem<IEnumerable<Order>>(Mapper.Map<IEnumerable<Order>>(message), basicDeliverEventArgs.DeliveryTag, channel));
                    }
                    catch (Exception e)
                    {
                        _log.Error(e);

                        channel.BasicReject(basicDeliverEventArgs.DeliveryTag, false);
                    }
                };

                var tag = channel.BasicConsume(queueName, false, consumer);
                
                while (!_cancellationTokenSource.IsCancellationRequested)
                    await Task.Delay(100);

                await _dbWriterTask;

                channel.BasicCancel(tag);
                connection.Close();
            }
        }

        private async Task StartDbWriter()
        {
            while (!_cancellationTokenSource.IsCancellationRequested || _queue.Count > 0)
            {
                var isFullBatch = false;
                try
                {
                    var list = new List<CustomQueueItem<IEnumerable<Order>>>();
                    try
                    {
                        for (var i = 0; i < ExecutionsBulkSize; i++)
                        {
                            if (_queue.TryDequeue(out var customQueueItem))
                                list.Add(customQueueItem);
                            else
                                break;
                        }

                        if (list.Count > 0)
                        {
                            isFullBatch = list.Count == ExecutionsBulkSize;

                            var orders = list.SelectMany(x => x.Value).ToList();

                            await _ordersRepository.UpsertBulkAsync(orders);

                            var trades = orders.SelectMany(x => x.Trades);

                            var batched = trades.Batch(TradesBulkSize).ToList();

                            foreach (var tradesBatch in batched)
                            {
                                await _historyRecordsRepository.InsertBulkAsync(tradesBatch);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error(e);

                        foreach (var item in list)
                            item.Reject();
                    }
                    finally
                    {
                        foreach (var item in list)
                            item.Accept();
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
                finally
                {
                    await Task.Delay(isFullBatch ? 1 : 1000);
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
