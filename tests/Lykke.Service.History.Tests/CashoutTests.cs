using System;
using System.Threading.Tasks;
using Autofac;
using Lykke.Cqrs;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Tests.Init;
using Lykke.Service.PostProcessing.Contracts.Cqrs;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
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

        private CashOutProcessedEvent CreateCashoutRecord()
        {
            var cqrs = _container.Resolve<ICqrsEngine>();

            var id = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var volume = 54.31M;

            var @event = new CashOutProcessedEvent
            {
                Id = id,
                WalletId = clientId,
                Volume = volume,
                AssetId = "EUR",
                Timestamp = DateTime.UtcNow
            };

            cqrs.PublishEvent(@event, PostProcessingBoundedContext.Name);

            return @event;
        }
    }
}
