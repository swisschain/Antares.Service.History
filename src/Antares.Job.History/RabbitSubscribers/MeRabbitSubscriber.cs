using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antares.Job.History.RabbitSubscribers.Events;
using Antares.Service.History.Core.Domain.Enums;
using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.Core.Settings;
using Autofac;
using AutoMapper;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.MatchingEngine.Connector.Models.Events;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Deduplication;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OrderModel = Antares.Job.History.RabbitSubscribers.Models.OrderModel;
using OrderSide = Antares.Job.History.RabbitSubscribers.Models.Enums.OrderSide;
using OrderStatus = Antares.Job.History.RabbitSubscribers.Models.Enums.OrderStatus;
using OrderType = Antares.Job.History.RabbitSubscribers.Models.Enums.OrderType;
using TradeModel = Antares.Job.History.RabbitSubscribers.Models.TradeModel;
using TradeRole = Antares.Job.History.RabbitSubscribers.Models.Enums.TradeRole;
using Utils = Antares.Service.History.Core.Utils;

namespace Antares.Job.History.RabbitSubscribers
{
    [UsedImplicitly]
    public class MeRabbitSubscriber : IStartable, IStopable
    {
        [NotNull] private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _rabbitMqSettings;
        [NotNull] private readonly CqrsSettings _cqrsSettings;
        private readonly ICqrsEngine _cqrsEngine;
        private readonly List<IStopable> _subscribers = new List<IStopable>();
        private readonly IDeduplicator _deduplicator;
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly IReadOnlyList<string> _walletIds;
        private readonly ILog _log;

        private const string QueueName = "lykke.spot.matching.engine.out.events.antares-history";
        private const bool QueueDurable = true;

        public MeRabbitSubscriber(
            [NotNull] ILogFactory logFactory,
            [NotNull] RabbitMqSettings rabbitMqSettings,
            [NotNull] CqrsSettings cqrsSettings,
            [NotNull] ICqrsEngine cqrsEngine,
            [NotNull] IDeduplicator deduplicator,
            IHistoryRecordsRepository historyRecordsRepository,
            IReadOnlyList<string> walletIds)
        {
            _logFactory = logFactory ?? throw new ArgumentNullException(nameof(logFactory));
            _log = _logFactory.CreateLog(this);
            _rabbitMqSettings = rabbitMqSettings ?? throw new ArgumentNullException(nameof(rabbitMqSettings));
            _cqrsSettings = cqrsSettings ?? throw new ArgumentNullException(nameof(cqrsSettings));
            _cqrsEngine = cqrsEngine ?? throw new ArgumentNullException(nameof(cqrsEngine));
            _deduplicator = deduplicator ?? throw new ArgumentNullException(nameof(deduplicator));
            _historyRecordsRepository = historyRecordsRepository;
            _walletIds = walletIds;
            var rabbitPublisher = new RabbitMqPublisher<ExecutionProcessedEvent>(_logFactory, new RabbitMqSubscriptionSettings()
            {
                ConnectionString = cqrsSettings.RabbitConnString,
                ExchangeName = _rabbitMqSettings.Exchange,
                RoutingKey = nameof(ExecutionProcessedEvent),
                IsDurable = QueueDurable
            });
        }

        public void Start()
        {
            _subscribers.Add(Subscribe<CashInEvent>(Lykke.MatchingEngine.Connector.Models.Events.Common.MessageType.CashIn, ProcessMessageAsync));
            _subscribers.Add(Subscribe<CashOutEvent>(Lykke.MatchingEngine.Connector.Models.Events.Common.MessageType.CashOut, ProcessMessageAsync));
            _subscribers.Add(Subscribe<CashTransferEvent>(Lykke.MatchingEngine.Connector.Models.Events.Common.MessageType.CashTransfer, ProcessMessageAsync));
            _subscribers.Add(Subscribe<ExecutionEvent>(Lykke.MatchingEngine.Connector.Models.Events.Common.MessageType.Order, ProcessMessageAsync));
        }

        private RabbitMqSubscriber<T> Subscribe<T>(Lykke.MatchingEngine.Connector.Models.Events.Common.MessageType messageType, Func<T, Task> func)
        {
            var settings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = _rabbitMqSettings.ConnectionString,
                QueueName = $"{QueueName}.{messageType}",
                ExchangeName = _rabbitMqSettings.Exchange,
                RoutingKey = ((int)messageType).ToString(),
                IsDurable = QueueDurable
            };

            return new RabbitMqSubscriber<T>(
                    _logFactory,
                    settings,
                    new ResilientErrorHandlingStrategy(_logFactory, settings,
                        retryTimeout: TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_logFactory, settings)))
                .SetMessageDeserializer(new ProtobufMessageDeserializer<T>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(func)
                .CreateDefaultBinding()
                .SetAlternativeExchange(_rabbitMqSettings.AlternativeConnectionString)
                .SetDeduplicator(_deduplicator)
                .Start();
        }


        private async Task ProcessMessageAsync(CashInEvent message)
        {
            var fees = message.CashIn.Fees;
            var fee = fees?.FirstOrDefault()?.Transfer;

            var entity = new Cashin()
            {
                AssetId = message.CashIn.AssetId,
                //BlockchainHash = ,
                FeeSize = ParseNullabe(fee?.Volume),
                Id = Guid.Parse(message.Header.RequestId),
                State = HistoryState.Finished,
                Timestamp = message.Header.Timestamp,
                Volume = Math.Abs(decimal.Parse(message.CashIn.Volume)),
                WalletId = Guid.Parse(message.CashIn.WalletId)
            };

            if (!await _historyRecordsRepository.TryInsertAsync(entity))
                _log.Warning($"Skipped duplicated cashin record", context: new
                {
                    id = message.Header.RequestId
                });
        }

        private async Task ProcessMessageAsync(CashOutEvent message)
        {
            var fees = message.CashOut.Fees;
            var fee = fees?.FirstOrDefault()?.Transfer;

            var entity = new Cashout()
            {
                //BlockchainHash = ,
                FeeSize = ParseNullabe(fee?.Volume),
                State = HistoryState.Finished,
                Id = Guid.Parse(message.Header.RequestId),
                WalletId = Guid.Parse(message.CashOut.WalletId),
                AssetId = message.CashOut.AssetId,
                Timestamp = message.Header.Timestamp,
                Volume = -Math.Abs(decimal.Parse(message.CashOut.Volume))
            };

            if (!await _historyRecordsRepository.TryInsertAsync(entity))
                _log.Warning($"Skipped duplicated cashout record", context: new
                {
                    id = message.Header.RequestId
                });
        }

        private async Task ProcessMessageAsync(CashTransferEvent message)
        {
            var fees = message.CashTransfer.Fees;
            var fee = fees?.FirstOrDefault()?.Transfer;
            var operationId = Guid.Parse(message.Header.RequestId);
            var fromWallet = Guid.Parse(message.CashTransfer.FromWalletId);
            var toWallet = Guid.Parse(message.CashTransfer.ToWalletId);
            var volume = decimal.Parse(message.CashTransfer.Volume);
            var timestamp = message.Header.Timestamp;
            var assetId = message.CashTransfer.AssetId;
            var feeSourceWalletId = fee != null ? Guid.Parse(fee.SourceWalletId) : (Guid?)null;
            var feeSize = ParseNullabe(fee?.Volume);

            var cashInOuts = new BaseHistoryRecord[] {
                new Cashout()
                {
                    Id = operationId,
                    WalletId = fromWallet,
                    Volume = -Math.Abs(volume),
                    Timestamp = timestamp,
                    AssetId = assetId,
                    FeeSize = fromWallet == feeSourceWalletId ? feeSize : null,
                    State = Antares.Service.History.Core.Domain.Enums.HistoryState.Finished
                },
                new Cashin()
                {
                    Id = operationId,
                    WalletId = toWallet,
                    Volume = Math.Abs(volume),
                    Timestamp = timestamp,
                    AssetId = assetId,
                    FeeSize = toWallet == feeSourceWalletId ? feeSize : null,
                    State = Antares.Service.History.Core.Domain.Enums.HistoryState.Finished
                }};

            foreach (var cashInOut in cashInOuts)
                if (!await _historyRecordsRepository.TryInsertAsync(cashInOut))
                    _log.Warning($"Skipped duplicated transfer record", context: new
                    {
                        id = operationId
                    });
        }

        private Task ProcessMessageAsync(ExecutionEvent message)
        {
            var orders = message.Orders.Select(x =>
            {
                var order = new Antares.Service.History.Core.Domain.Orders.Order
                {
                    Id = Guid.Parse(x.ExternalId),
                    WalletId = Guid.Parse(x.WalletId),
                    Volume = decimal.Parse(x.Volume),
                    AssetPairId = x.AssetPairId,
                    CreateDt = x.CreatedAt,
                    LowerLimitPrice = ParseNullabe(x.LowerLimitPrice),
                    LowerPrice = ParseNullabe(x.LowerPrice),
                    MatchDt = x.LastMatchTime,
                    MatchingId = Guid.Parse(x.Id),
                    Price = ParseNullabe(x.Price),
                    RegisterDt = x.Registered,
                    RejectReason = x.RejectReason,
                    RemainingVolume = ParseNullabe(x.RemainingVolume) ?? 0m,
                    Side = (Antares.Service.History.Core.Domain.Enums.OrderSide)(int)x.Side,
                    Status = (Antares.Service.History.Core.Domain.Enums.OrderStatus)(int)x.Status,
                    StatusDt = x.StatusDate,
                    Straight = x.OrderType == Lykke.MatchingEngine.Connector.Models.Events.OrderType.Limit ||
                               x.OrderType == Lykke.MatchingEngine.Connector.Models.Events.OrderType.StopLimit || x.Straight,
                    SequenceNumber = message.Header.SequenceNumber,
                    Type = (Antares.Service.History.Core.Domain.Enums.OrderType)(int)x.OrderType,
                    UpperLimitPrice = ParseNullabe(x.UpperLimitPrice),
                    UpperPrice = ParseNullabe(x.UpperPrice),
                };

                order.Trades = x.Trades?.Select(t => new Antares.Service.History.Core.Domain.History.Trade()
                {
                    Id = Guid.Parse(t.TradeId),
                    WalletId = Guid.Parse(x.WalletId),
                    AssetPairId = x.AssetPairId,
                    BaseAssetId = t.BaseAssetId,
                    BaseVolume = decimal.Parse(t.BaseVolume),
                    Price = decimal.Parse(t.Price),
                    Timestamp = t.Timestamp,
                    QuotingAssetId = t.QuotingAssetId,
                    QuotingVolume = decimal.Parse(t.QuotingVolume),
                    Index = t.Index,
                    Role = (Antares.Service.History.Core.Domain.Enums.TradeRole)(int)t.Role,
                    FeeSize = ParseNullabe(t.Fees?.FirstOrDefault()?.Volume),
                    FeeAssetId = t.Fees?.FirstOrDefault()?.AssetId,
                    OrderId = order.Id
                    //OppositeWalletId = Guid.Parse(t.OppositeWalletId),
                }).ToList();

                return order;
            }).ToList();

            foreach (var order in orders.Where(x => _walletIds.Contains(x.WalletId.ToString())))
            {
                _log.Info("Order from ME", $"order: {new { order.Id, order.Status, message.Header.SequenceNumber }.ToJson()}");
            }

            var limitOrders = orders.Where(x => x.Type == Antares.Service.History.Core.Domain.Enums.OrderType.Limit ||
                                                x.Type == Antares.Service.History.Core.Domain.Enums.OrderType.StopLimit).ToList();
            var historyOrders = new List<OrderEvent>(limitOrders.Count);



            foreach (var order in limitOrders)
            {
                switch (order.Status)
                {
                    case Service.History.Core.Domain.Enums.OrderStatus.Cancelled:
                        {
                            var orderEvent = new OrderEvent()
                            {
                                OrderId = order.Id,
                                Status = order.Status,
                                AssetPairId = order.AssetPairId,
                                Price = order.Price ?? 0,
                                Timestamp = order.StatusDt,
                                Volume = order.Volume,
                                WalletId = order.WalletId,
                                Id = Utils.IncrementGuid(order.Id, (int)Service.History.Core.Domain.Enums.OrderStatus.Cancelled)
                            };

                            historyOrders.Add(orderEvent);

                            break;
                        }
                    case Service.History.Core.Domain.Enums.OrderStatus.Placed:
                    {
                        var orderEvent = new OrderEvent()
                        {
                            OrderId = order.Id,
                            Status = order.Status,
                            AssetPairId = order.AssetPairId,
                            Price = order.Price ?? 0,
                            Timestamp = order.CreateDt,
                            Volume = order.Volume,
                            WalletId = order.WalletId,
                            Id = Utils.IncrementGuid(order.Id, (int)Service.History.Core.Domain.Enums.OrderStatus.Placed)
                        };

                        historyOrders.Add(orderEvent);

                        break;
                    }
                    case Service.History.Core.Domain.Enums.OrderStatus.Matched:
                    case Service.History.Core.Domain.Enums.OrderStatus.PartiallyMatched:
                    {
                        if (!order.Trades.Any(t => t.Role == Service.History.Core.Domain.Enums.TradeRole.Taker))
                            break;
                        var orderEvent = new OrderEvent()
                        {
                            OrderId = order.Id,
                            Status = Service.History.Core.Domain.Enums.OrderStatus.Placed,
                            AssetPairId = order.AssetPairId,
                            Price = order.Price ?? 0,
                            Timestamp = order.CreateDt,
                            Volume = order.Volume,
                            WalletId = order.WalletId,
                            Id = Utils.IncrementGuid(order.Id, (int)Service.History.Core.Domain.Enums.OrderStatus.Placed)
                        };

                        historyOrders.Add(orderEvent);

                        break;
                    }
                    default:
                        continue;
                }
            }

            return Task.CompletedTask;
        }

        private decimal? ParseNullabe(string value)
        {
            return !string.IsNullOrEmpty(value) ? decimal.Parse(value) : (decimal?)null;
        }

        public void Dispose()
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber?.Dispose();
            }

        }

        public void Stop()
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber?.Stop();
            }
        }
    }
}
