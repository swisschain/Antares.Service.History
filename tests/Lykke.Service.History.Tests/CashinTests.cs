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
    public class CashinTests
    {
        private readonly IContainer _container;

        public CashinTests(TestInitialization initialization)
        {
            _container = initialization.Container;
        }

        [Fact]
        public async Task SaveCashin_Test()
        {
            var command = CreateCashinRecord();

            await Task.Delay(3000);

            var repo = _container.Resolve<IHistoryRecordsRepository>();

            var item = await repo.Get(command.Id, command.WalletId);

            Assert.NotNull(item);
            Assert.True(item is Cashin);

            var cashin = item as Cashin;

            Assert.Equal(command.FeeSize, cashin.FeeSize);
            Assert.Equal(Math.Abs(command.Volume), cashin.Volume);
        }

        private CashInProcessedEvent CreateCashinRecord()
        {
            var cqrs = _container.Resolve<ICqrsEngine>();

            var id = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var volume = 54.31M;

            var @event = new CashInProcessedEvent
            {
                Id = id,
                WalletId = clientId,
                Volume = volume,
                AssetId = "EUR",
                Timestamp = DateTime.UtcNow,
                FeeSize = 10
            };

            cqrs.PublishEvent(@event, PostProcessingBoundedContext.Name);

            return @event;
        }
    }
}
