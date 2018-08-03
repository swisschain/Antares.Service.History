using System;
using System.Threading.Tasks;
using Autofac;
using Lykke.Cqrs;
using Lykke.Service.History.Contracts.Cqrs;
using Lykke.Service.History.Contracts.Cqrs.Commands;
using Lykke.Service.History.Core.Domain;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Tests.Init;
using Xunit;

namespace Lykke.Service.History.Tests
{
    [Collection("history-tests")]
    public class CashoutTests
    {
        private readonly IContainer _container;

        public CashoutTests(TestInitialization initialization)
        {
            _container = initialization.Container;
        }

        [Fact]
        public async Task SaveCashout_Test()
        {
            var command = CreateCashoutRecord();

            await Task.Delay(3000);

            var repo = _container.Resolve<IHistoryRecordsRepository>();

            var item = await repo.Get(command.Id, command.WalletId);

            Assert.NotNull(item);
            Assert.True(item is Cashout);
            Assert.Equal(-Math.Abs(command.Volume), (item as Cashout).Volume);
        }

        private SaveCashoutCommand CreateCashoutRecord()
        {
            var cqrs = _container.Resolve<ICqrsEngine>();

            var id = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var volume = 54.31M;

            var command = new SaveCashoutCommand
            {
                Id = id,
                WalletId = clientId,
                Volume = volume,
                AssetId = "EUR",
                Timestamp = DateTime.UtcNow
            };

            cqrs.SendCommand(command, BoundedContext.Name, BoundedContext.Name);

            return command;
        }
    }
}
