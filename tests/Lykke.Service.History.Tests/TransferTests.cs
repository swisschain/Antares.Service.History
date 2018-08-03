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
    public class TransferTests
    {
        private readonly IContainer _container;

        public TransferTests(TestInitialization initialization)
        {
            _container = initialization.Container;
        }

        [Fact]
        public async Task SaveTransfer_Test()
        {
            var command = CreateTransferRecord();

            await Task.Delay(3000);

            var repo = _container.Resolve<IHistoryRecordsRepository>();

            var itemFrom = await repo.Get(command.Id, command.FromWalletId);
            var itemTo = await repo.Get(command.Id, command.ToWalletId);

            Assert.NotNull(itemFrom);
            Assert.NotNull(itemTo);

            Assert.True(itemFrom is Transfer);
            Assert.True(itemTo is Transfer);

            var transferFrom = itemFrom as Transfer;
            var transferTo = itemTo as Transfer;

            Assert.Equal(-Math.Abs(command.Volume), transferFrom.Volume);
            Assert.Equal(Math.Abs(command.Volume), transferTo.Volume);

            Assert.Equal(0, transferFrom.FeeSize);
            Assert.Equal(command.FeeSize, transferTo.FeeSize);
        }

        private SaveTransferCommand CreateTransferRecord()
        {
            var cqrs = _container.Resolve<ICqrsEngine>();

            var id = Guid.NewGuid();
            var walletFrom = Guid.NewGuid();
            var walletTo = Guid.NewGuid();
            var volume = new Random().Next(1, 100);

            var command = new SaveTransferCommand
            {
                Id = id,
                FromWalletId = walletFrom,
                ToWalletId = walletTo,
                Volume = volume,
                AssetId = "EUR",
                Timestamp = DateTime.UtcNow,
                FeeSize = 0.5M,
                FeeWalletId = walletTo
            };

            cqrs.SendCommand(command, BoundedContext.Name, BoundedContext.Name);

            return command;
        }
    }
}

