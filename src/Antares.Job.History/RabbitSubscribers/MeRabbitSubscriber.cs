using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Models;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Models.Enums;
using OrderStatus = Lykke.Service.PostProcessing.Contracts.Cqrs.Models.Enums.OrderStatus;
using OrderType = Lykke.Service.PostProcessing.Contracts.Cqrs.Models.Enums.OrderType;
using TradeRole = Lykke.Service.PostProcessing.Contracts.Cqrs.Models.Enums.TradeRole;

namespace Antares.Job.History.RabbitSubscribers
{
    [UsedImplicitly]
    public class MeRabbitSubscriber : IStartable, IStopable
    {
        [NotNull] private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _rabbitMqSettings;
        private readonly ICqrsEngine _cqrsEngine;
        private readonly List<IStopable> _subscribers = new List<IStopable>();
        private readonly IDeduplicator _deduplicator;
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly IReadOnlyList<string> _walletIds;
        private readonly ILog _log;

        private const string QueueName = "lykke.spot.matching.engine.out.events.post-processing";
        private const bool QueueDurable = true;

        public MeRabbitSubscriber(
            [NotNull] ILogFactory logFactory,
            [NotNull] RabbitMqSettings rabbitMqSettings,
            [NotNull] ICqrsEngine cqrsEngine,
            [NotNull] IDeduplicator deduplicator,
            IHistoryRecordsRepository _historyRecordsRepository,
            IReadOnlyList<string> walletIds)
        {
            _logFactory = logFactory ?? throw new ArgumentNullException(nameof(logFactory));
            _log = _logFactory.CreateLog(this);
            _rabbitMqSettings = rabbitMqSettings ?? throw new ArgumentNullException(nameof(rabbitMqSettings));
            _cqrsEngine = cqrsEngine ?? throw new ArgumentNullException(nameof(cqrsEngine));
            _deduplicator = deduplicator ?? throw new ArgumentNullException(nameof(deduplicator));
            this._historyRecordsRepository = _historyRecordsRepository;
            _walletIds = walletIds;
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
            var feeSourceWalletId = fee != null ? Guid.Parse(fee.SourceWalletId) : (Guid?) null;
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
            var orders = message.Orders.Select(x => new OrderModel
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
                RemainingVolume = ParseNullabe(x.RemainingVolume),
                Side = (Lykke.Service.PostProcessing.Contracts.Cqrs.Models.Enums.OrderSide)(int)x.Side,
                Status = (Lykke.Service.PostProcessing.Contracts.Cqrs.Models.Enums.OrderStatus)(int)x.Status,
                StatusDt = x.StatusDate,
                Straight = x.OrderType == OrderType.Limit || x.OrderType == OrderType.StopLimit || x.Straight,
                Type = (Lykke.Service.PostProcessing.Contracts.Cqrs.Models.Enums.OrderType)(int)x.OrderType,
                UpperLimitPrice = ParseNullabe(x.UpperLimitPrice),
                UpperPrice = ParseNullabe(x.UpperPrice),
                Trades = x.Trades?.Select(t => new TradeModel
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
                    Role = (Lykke.Service.PostProcessing.Contracts.Cqrs.Models.Enums.TradeRole)(int)t.Role,
                    FeeSize = ParseNullabe(t.Fees?.FirstOrDefault()?.Volume),
                    FeeAssetId = t.Fees?.FirstOrDefault()?.AssetId,
                    OppositeWalletId = Guid.Parse(t.OppositeWalletId),
                }).ToList()
            }).ToList();

            foreach (var order in orders.Where(x => _walletIds.Contains(x.WalletId.ToString())))
            {
                _log.Info("Order from ME", $"order: {new { order.Id, order.Status, message.Header.SequenceNumber }.ToJson()}");
            }

            var @event = new ExecutionProcessedEvent
            {
                SequenceNumber = message.Header.SequenceNumber,
                Orders = orders
            };
            _cqrsEngine.PublishEvent(@event, BoundedContext.Name);

            foreach (var order in orders.Where(x => x.Trades != null && x.Trades.Any()))
            {
                var tradeProcessedEvent = new ManualOrderTradeProcessedEvent
                {
                    Order = order
                };
                _cqrsEngine.PublishEvent(tradeProcessedEvent, BoundedContext.Name);
            }

            foreach (var order in message.Orders.Where(x => x.Trades != null && x.Trades.Count > 0))
            {
                var orderType = order.OrderType == OrderType.Market ? FeeOperationType.Trade : FeeOperationType.LimitTrade;
                var orderId = order.Id;
                foreach (var trade in order.Trades)
                {
                    if (trade.Fees != null)
                    {
                        var feeEvent = new FeeChargedEvent
                        {
                            OperationId = orderId,
                            OperationType = orderType,
                            Fee = trade.Fees.ToJson()
                        };
                        _cqrsEngine.PublishEvent(feeEvent, BoundedContext.Name);
                    }
                }
            }

            var limitOrders = orders.Where(x => x.Type == Lykke.Service.PostProcessing.Contracts.Cqrs.Models.Enums.OrderType.Limit ||
                                                x.Type == Lykke.Service.PostProcessing.Contracts.Cqrs.Models.Enums.OrderType.StopLimit).ToList();
            foreach (var order in limitOrders.Where(x => x.Status == OrderStatus.Cancelled))
            {
                var orderCancelledEvent = new OrderCancelledEvent
                {
                    OrderId = order.Id,
                    Status = order.Status,
                    AssetPairId = order.AssetPairId,
                    Price = order.Price,
                    Timestamp = order.StatusDt,
                    Volume = order.Volume,
                    WalletId = order.WalletId
                };
                _cqrsEngine.PublishEvent(orderCancelledEvent, BoundedContext.Name);
            }

            foreach (var order in limitOrders.Where(x => x.Status == OrderStatus.Placed))
            {
                var orderPlacedEvent = new OrderPlacedEvent
                {
                    OrderId = order.Id,
                    Status = order.Status,
                    AssetPairId = order.AssetPairId,
                    Price = order.Price,
                    Timestamp = order.StatusDt,
                    Volume = order.Volume,
                    WalletId = order.WalletId,
                    CreateDt = order.CreateDt
                };
                _cqrsEngine.PublishEvent(orderPlacedEvent, BoundedContext.Name);
            }

            foreach (var order in limitOrders.Where(x =>
                (x.Status == OrderStatus.Matched || x.Status == OrderStatus.PartiallyMatched)
                && x.Trades.Any(t => t.Role == TradeRole.Taker)))
            {
                var orderPlacedEvent = new OrderPlacedEvent
                {
                    OrderId = order.Id,
                    Status = order.Status,
                    AssetPairId = order.AssetPairId,
                    Price = order.Price,
                    Timestamp = order.StatusDt,
                    Volume = order.Volume,
                    WalletId = order.WalletId,
                    CreateDt = order.CreateDt
                };
                _cqrsEngine.PublishEvent(orderPlacedEvent, BoundedContext.Name);
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
