using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Dapper;
using Dommel;
using Lykke.Cqrs;
using Lykke.Service.History.Contracts.Cqrs;
using Lykke.Service.History.Contracts.Cqrs.Commands;
using Lykke.Service.History.Core.Domain;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.PostgresRepositories.Entities;
using Lykke.Service.History.Tests.Init;
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

        private SaveCashinCommand CreateCashinRecord()
        {
            var cqrs = _container.Resolve<ICqrsEngine>();

            var id = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var volume = 54.31M;

            var command = new SaveCashinCommand
            {
                Id = id,
                WalletId = clientId,
                Volume = volume,
                AssetId = "EUR",
                Timestamp = DateTime.UtcNow,
                FeeSize = 10
            };

            cqrs.SendCommand(command, BoundedContext.Name, BoundedContext.Name);

            return command;
        }
    }
}
