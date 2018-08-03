using System;
using System.Collections.Generic;
using Autofac;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Service.History.AutoMapper;
using Lykke.Service.History.Contracts.Cqrs;
using Lykke.Service.History.Contracts.Cqrs.Commands;
using Lykke.Service.History.Core.Domain;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.History.Modules;
using Lykke.Service.History.PostgresRepositories.Mappings;
using Lykke.Service.History.Settings;
using Lykke.Service.History.Workflow.Handlers;
using Lykke.SettingsReader.ReloadingManager;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Lykke.Service.History.Tests.Init
{
    [CollectionDefinition("history-tests")]
    public class TestInitializationCollection : ICollectionFixture<TestInitialization>
    {
    }

    public class TestInitialization
    {
        static TestInitialization()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfiles(typeof(ServiceProfile));
                cfg.AddProfiles(typeof(RepositoryProfile));
            });
        }

        public IContainer Container { get; }

        public TestInitialization()
        {
            Container = CreateContainer();
        }

        public IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(LogFactory.Create().AddUnbufferedConsole()).As<ILogFactory>().SingleInstance();

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

            builder.RegisterType<CashinCommandHandler>();
            builder.RegisterType<CashoutCommandHandler>();
            builder.RegisterType<TransferCommandHandler>();
            builder.RegisterType<ExecutionCommandHandler>();

            builder.Register(ctx =>
                {
                    var logFactory = ctx.Resolve<ILogFactory>();
                    var messagingEngine = new MessagingEngine(
                        logFactory,
                        new TransportResolver(new Dictionary<string, TransportInfo>
                        {
                            {"InMemory", new TransportInfo("none", "none", "none", null, "InMemory")}
                        }));
                    return CreateEngine(ctx, messagingEngine, logFactory);
                })
                .As<ICqrsEngine>()
                .AutoActivate()
                .SingleInstance();

            RegisterRepo(builder);

            return builder.Build();
        }

        private void RegisterRepo(ContainerBuilder builder)
        {
            var connection = Environment.GetEnvironmentVariable("PostgresConnection");

            if (string.IsNullOrWhiteSpace(connection))
            {
                builder.RegisterType<InMemoryHistoryRepository>()
                    .As<IHistoryRecordsRepository>()
                    .SingleInstance();

                builder.RegisterType<InMemoryOrdersRepository>()
                    .As<IOrdersRepository>()
                    .SingleInstance();
            }
            else
            {
                var settings = new AppSettings()
                {
                    HistoryService = new HistorySettings
                    {
                        Db = new DbSettings
                        {
                            PostgresDataConnString = connection
                        }
                    }
                };

                builder.RegisterModule(new DbModule(ConstantReloadingManager.From(settings)));
            }
        }

        private CqrsEngine CreateEngine(
            IComponentContext ctx,
            IMessagingEngine messagingEngine,
            ILogFactory logFactory)
        {
            const string defaultRoute = "commands";

            return new CqrsEngine(
                logFactory,
                ctx.Resolve<IDependencyResolver>(),
                messagingEngine,
                new DefaultEndpointProvider(),
                true,
                Register.DefaultEndpointResolver(new InMemoryEndpointResolver()),

                Register.BoundedContext(BoundedContext.Name)
                    .ListeningCommands(typeof(SaveCashinCommand))
                    .On(defaultRoute)
                    .WithLoopback()
                    .WithCommandsHandler<CashinCommandHandler>()

                    .ListeningCommands(typeof(SaveCashoutCommand))
                    .On(defaultRoute)
                    .WithLoopback()
                    .WithCommandsHandler<CashoutCommandHandler>()

                    .ListeningCommands(typeof(SaveExecutionCommand))
                    .On(defaultRoute)
                    .WithLoopback()
                    .WithCommandsHandler<ExecutionCommandHandler>()

                    .ListeningCommands(typeof(SaveTransferCommand))
                    .On(defaultRoute)
                    .WithLoopback()
                    .WithCommandsHandler<TransferCommandHandler>());
        }
    }
}
