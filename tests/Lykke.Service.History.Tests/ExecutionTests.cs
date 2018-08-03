using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Lykke.Cqrs;
using Lykke.Service.History.Contracts.Cqrs;
using Lykke.Service.History.Contracts.Cqrs.Commands;
using Lykke.Service.History.Contracts.Cqrs.Models;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.History.Tests.Init;
using Xunit;
using OrderSide = Lykke.Service.History.Contracts.Cqrs.Models.Enums.OrderSide;
using OrderStatus = Lykke.Service.History.Contracts.Cqrs.Models.Enums.OrderStatus;
using OrderType = Lykke.Service.History.Contracts.Cqrs.Models.Enums.OrderType;
using TradeRole = Lykke.Service.History.Contracts.Cqrs.Commands.Models.TradeRole;

namespace Lykke.Service.History.Tests
{
    [Collection("history-tests")]
    public class ExecutionTests
    {
        private readonly IContainer _container;

        public ExecutionTests(TestInitialization initialization)
        {
            _container = initialization.Container;
        }

        [Fact]
        public async Task SaveExecution_Test()
        {
            var walletId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var tradeId = Guid.NewGuid();
            var date = DateTime.UtcNow;

            var tradeModel = new TradeModel
            {
                Id = tradeId,
                WalletId = walletId,
                AssetPairId = "BTCUSD",
                AssetId = "BTC",
                Volume = 5,
                OppositeAssetId = "USD",
                OppositeVolume = -5002,
                Price = 10001,
                Role = TradeRole.Maker,
                Timestamp = date.AddMilliseconds(1)
            };

            var orderModel = new OrderModel
            {
                Id = orderId,
                AssetPairId = "BTCUSD",
                CreateDt = date,
                StatusDt = date,
                MatchingId = Guid.NewGuid(),
                Price = 10001.123M,
                Volume = 10,
                RemainingVolume = 10,
                Side = OrderSide.Buy,
                Status = OrderStatus.Placed,
                Straight = true,
                Type = OrderType.Limit,
                WalletId = walletId,
                Trades = new List<TradeModel> { tradeModel }
            };

            var command = new SaveExecutionCommand
            {
                SequenceNumber = 100,
                Orders = new List<OrderModel>
                {
                    orderModel
                }
            };

            var cqrs = _container.Resolve<ICqrsEngine>();

            cqrs.SendCommand(command, BoundedContext.Name, BoundedContext.Name);

            await Task.Delay(3000);

            var repo = _container.Resolve<IHistoryRecordsRepository>();
            var orderRepo = _container.Resolve<IOrdersRepository>();


            var order = await orderRepo.Get(orderModel.Id);

            Assert.NotNull(order);
            Assert.Equal((int)order.Status, (int)orderModel.Status);
            Assert.Equal(order.SequenceNumber, command.SequenceNumber);
            Assert.Equal(order.Volume, orderModel.Volume);

            var item = await repo.Get(tradeModel.Id, tradeModel.WalletId);

            Assert.NotNull(item);
            Assert.True(item is Trade);
            Assert.Equal(Math.Abs(tradeModel.Volume), (item as Trade).Volume);
        }
    }
}
