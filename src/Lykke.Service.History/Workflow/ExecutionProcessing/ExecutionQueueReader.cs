using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Log;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.History.PostgresRepositories;
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

        private readonly ILogFactory _logFactory;
        private readonly string _connectionString;
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly IOrdersRepository _ordersRepository;

        private ConcurrentQueue<KeyValuePair<List<Order>, MessageAcceptor>> _queue;

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
            _queue = new ConcurrentQueue<KeyValuePair<List<Order>, MessageAcceptor>>();
            _saveTask = StartSaving();
            
            var queueName = "lykke.history.queue.post-processing.events.reader.projections";

            var factory = new ConnectionFactory
            {
                Uri = _connectionString
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueBind(
                    queue: queueName,
                    exchange: "lykke.post-processing.events.exchange",
                    routingKey: "ExecutionProcessedEvent");

                channel.BasicQos(0, ExecutionsBulkSize * 2, false);

                channel.QueueDeclare(queueName, true, false, false, null);

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (o, a) =>
                   {
                       var message = ProtobufMessageDeserializer<ExecutionProcessedEvent>.Deserialize(a.Body);

                       _queue.Enqueue(KeyValuePair.Create(Mapper.Map<IEnumerable<Order>>(message).ToList(),
                           new MessageAcceptor(channel, a.DeliveryTag)));
                   };

                var tag = channel.BasicConsume(queueName, false, consumer);

                while (!_cancellationTokenSource.IsCancellationRequested)
                    Task.Delay(100).GetAwaiter().GetResult();

                channel.BasicCancel(tag);
                connection.Close();
            }
        }

        public void Stop()
        {
            //_subscriber?.Stop();
            _cancellationTokenSource.Cancel();
        }

        private async Task StartSaving()
        {
            while (!_cancellationTokenSource.IsCancellationRequested || _queue.Count > 0)
            {
                var list = new List<KeyValuePair<List<Order>, MessageAcceptor>>();

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
                        item.Value.Accept();
                }

                await Task.Delay(10);
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public class ProtobufMessageDeserializer<TMessage>
    {
        public static TMessage Deserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return ProtoBuf.Serializer.Deserialize<TMessage>(stream);
            }
        }
    }
}
