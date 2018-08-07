using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.History.Tests.Init;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Models;
using MoreLinq;
using Xunit;
using OrderSide = Lykke.Service.PostProcessing.Contracts.Cqrs.Models.Enums.OrderSide;
using OrderStatus = Lykke.Service.PostProcessing.Contracts.Cqrs.Models.Enums.OrderStatus;
using OrderType = Lykke.Service.PostProcessing.Contracts.Cqrs.Models.Enums.OrderType;

namespace Lykke.Service.History.Tests
{
    [Collection("history-tests")]
    public class SelectDataTests
    {
        private readonly IContainer _container;

        public SelectDataTests(TestInitialization initialization)
        {
            _container = initialization.Container;
        }

        //[Fact]
        public async Task InsertOrders_Test()
        {
            var repo = _container.Resolve<IOrdersRepository>();
            var repotrades = _container.Resolve<IHistoryRecordsRepository>();

            var orders = Enumerable.Range(1, 1000).Select(x => GetOrder());

            var sw = Stopwatch.StartNew();

            foreach (var bulk in orders.Batch(100))
            {
                await repo.UpsertBulkAsync(bulk);

                await repotrades.TryInsertBulkAsync(bulk.SelectMany(x => x.Trades));

                Console.WriteLine(sw.ElapsedMilliseconds);
            }

            sw.Stop();
        }

        [Fact]
        public async Task SqlSelect_Test()
        {
            var pairs = new[] { "BTCUSD", "EURUSD", "ETHUSD" };
            var assets = new[] { "BTC", "USD", "EUR", "ETH" };

            var repo = _container.Resolve<IHistoryRecordsRepository>();

            var (walletId, data) = await GenerateData(pairs, assets);

            var q1 = await repo.GetByWallet(walletId, new[] { HistoryType.CashIn, HistoryType.CashOut }, 0, 100);

            Assert.Equal(data.OfType<Cashin>().Count() + data.OfType<Cashout>().Count(), q1.Count());

            var q2 = await repo.GetByWallet(walletId, new[] { HistoryType.Trade }, 0, 100);

            Assert.Equal(data.OfType<Trade>().Count(), q2.Count());

            var q3 = await repo.GetByWallet(walletId, new[] { HistoryType.CashIn, HistoryType.CashOut, HistoryType.Trade }, 0, 1);

            Assert.Single(q3);

            var q4 = await repo.GetByWallet(walletId, new[] { HistoryType.Trade }, 0, 100, assetId: "USD");

            Assert.Equal(data.OfType<Trade>().Count(x => x.AssetId == "USD" || x.OppositeAssetId == "USD"), q4.Count());

            var q5 = await repo.GetByWallet(walletId, new[] { HistoryType.Trade }, 0, 100, assetpairId: "BTCUSD");

            Assert.Equal(data.OfType<Trade>().Count(x => x.AssetPairId == "BTCUSD"), q5.Count());

        }


        private async Task<(Guid, List<BaseHistoryRecord>)> GenerateData(string[] pairs, string[] assets)
        {
            var walletId = Guid.NewGuid();
            var random = new Random();

            const int cashinsCount = 5;
            const int cashoutsCount = 3;
            const int tradesCount = 20;

            var list = new List<BaseHistoryRecord>();

            for (var i = 0; i < cashinsCount; i++)
            {
                var cashin = new Cashin()
                {
                    Id = Guid.NewGuid(),
                    WalletId = walletId,
                    AssetId = assets[random.Next(assets.Length)],
                    State = HistoryState.Finished,
                    Timestamp = DateTime.UtcNow,
                    Volume = random.Next(1, 100)
                };

                list.Add(cashin);
            }

            for (var i = 0; i < cashoutsCount; i++)
            {
                var cashout = new Cashout()
                {
                    Id = Guid.NewGuid(),
                    WalletId = walletId,
                    AssetId = assets[random.Next(assets.Length)],
                    State = HistoryState.Finished,
                    Timestamp = DateTime.UtcNow,
                    Volume = -random.Next(1, 100)
                };

                list.Add(cashout);
            }

            for (var i = 0; i < tradesCount; i++)
            {
                var cashout = new Trade()
                {
                    Id = Guid.NewGuid(),
                    WalletId = walletId,
                    AssetId = assets[random.Next(assets.Length)],
                    Timestamp = DateTime.UtcNow,
                    Volume = -random.Next(1, 100),
                    OppositeAssetId = assets[random.Next(assets.Length)],
                    OppositeVolume = random.Next(1, 50),
                    AssetPairId = pairs[random.Next(pairs.Length)],
                    OrderId = Guid.NewGuid(),
                    Price = random.Next() * 100
                };

                list.Add(cashout);
            }

            var repo = _container.Resolve<IHistoryRecordsRepository>();

            await repo.TryInsertBulkAsync(list);

            return (walletId, list);
        }

        private Order GetOrder()
        {
            var walletId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var tradeId = Guid.NewGuid();
            var date = DateTime.UtcNow;

            var random = new Random();

            Trade GetTrade()
            {
                return new Trade
                {
                    Id = Guid.NewGuid(),
                    WalletId = walletId,
                    AssetPairId = "BTCUSD",
                    AssetId = "BTC",
                    Volume = 5,
                    OppositeAssetId = "USD",
                    OppositeVolume = -5002,
                    Price = 10001,
                    Timestamp = date.AddMilliseconds(1)
                };
            }

            var orderModel = new Order
            {
                Id = orderId,
                AssetPairId = "BTCUSD",
                CreateDt = date,
                StatusDt = date,
                MatchingId = Guid.NewGuid(),
                Price = 10001.123M,
                Volume = 10,
                RemainingVolume = 10,
                Side = Core.Domain.Enums.OrderSide.Buy,
                Status = Core.Domain.Enums.OrderStatus.Placed,
                Straight = true,
                Type = Core.Domain.Enums.OrderType.Limit,
                WalletId = walletId,
                Trades = Enumerable.Range(1, random.Next(1, 20)).Select(x => GetTrade()).ToList()
            };

            return orderModel;
        }
    }
}
