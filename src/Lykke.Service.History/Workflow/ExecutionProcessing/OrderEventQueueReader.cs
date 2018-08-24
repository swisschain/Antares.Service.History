using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.PostProcessing.Contracts.Cqrs;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Lykke.Service.History.Workflow.ExecutionProcessing
{
    public class OrderEventQueueReader : IDisposable
    {
        private const int OrderEventBulkSize = 100;

        private readonly string _connectionString;
        private readonly IHealthNotifier _healthNotifier;
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly ILog _log;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _dbWriterTask;

        private ConcurrentQueue<CustomQueueItem<OrderEvent>> _queue;
        private Task _queueReaderTask;


        public OrderEventQueueReader(
            ILogFactory logFactory,
            string connectionString,
            IHistoryRecordsRepository historyRecordsRepository,
            IHealthNotifier healthNotifier)
        {
            _connectionString = connectionString;
            _historyRecordsRepository = historyRecordsRepository;
            _healthNotifier = healthNotifier;
            _log = logFactory.CreateLog(this);
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _queue = new ConcurrentQueue<CustomQueueItem<OrderEvent>>();
            _dbWriterTask = StartDbWriter();
            _queueReaderTask = StartQueueReader().ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    _healthNotifier.Notify("Order event queue reader unexpectedly stopped");
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
            var queueName = $"lykke.history.queue.{PostProcessingBoundedContext.Name}.events.order-event-reader";

            var factory = new ConnectionFactory
            {
                Uri = _connectionString
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.BasicQos(0, OrderEventBulkSize * 2, false);

                channel.QueueDeclare(queueName, true, false, false);

                channel.QueueBind(queueName, exchangeName, nameof(OrderPlacedEvent));
                channel.QueueBind(queueName, exchangeName, nameof(OrderCancelledEvent));

                var consumer = new EventingBasicConsumer(channel);

                var orderPlacedDeserializer = new ProtobufMessageDeserializer<OrderPlacedEvent>();
                var orderCancelledDeserializer = new ProtobufMessageDeserializer<OrderCancelledEvent>();

                consumer.Received += (o, basicDeliverEventArgs) =>
                {
                    try
                    {
                        switch (basicDeliverEventArgs.RoutingKey)
                        {
                            case nameof(OrderPlacedEvent):
                                _queue.Enqueue(new CustomQueueItem<OrderEvent>(
                                    Mapper.Map<OrderEvent>(
                                        orderPlacedDeserializer.Deserialize(basicDeliverEventArgs.Body)),
                                    basicDeliverEventArgs.DeliveryTag, channel));
                                break;
                            case nameof(OrderCancelledEvent):
                                _queue.Enqueue(new CustomQueueItem<OrderEvent>(
                                    Mapper.Map<OrderEvent>(
                                        orderCancelledDeserializer.Deserialize(basicDeliverEventArgs.Body)),
                                    basicDeliverEventArgs.DeliveryTag, channel));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(basicDeliverEventArgs.RoutingKey);
                        }

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
                    var exceptionThrowed = false;
                    var list = new List<CustomQueueItem<OrderEvent>>();
                    try
                    {
                        for (var i = 0; i < OrderEventBulkSize; i++)
                            if (_queue.TryDequeue(out var customQueueItem))
                                list.Add(customQueueItem);
                            else
                                break;

                        if (list.Count > 0)
                        {
                            isFullBatch = list.Count == OrderEventBulkSize;

                            await _historyRecordsRepository.InsertBulkAsync(list.Select(x => x.Value));
                        }
                    }
                    catch (Exception e)
                    {
                        exceptionThrowed = true;

                        _log.Error(e);

                        foreach (var item in list)
                            item.Reject();
                    }
                    finally
                    {
                        if (!exceptionThrowed)
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
    }
}
