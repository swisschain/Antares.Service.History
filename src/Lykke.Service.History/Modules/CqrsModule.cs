using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Common.Log;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using Lykke.Service.History.Settings;
using Lykke.Service.History.Workflow.Projections;
using Lykke.Service.PostProcessing.Contracts.Cqrs;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
using Lykke.SettingsReader;

namespace Lykke.Service.History.Modules
{
    public class CqrsModule : Module
    {
        private readonly CqrsSettings _settings;

        public CqrsModule(IReloadingManager<AppSettings> settingsManager)
        {
            _settings = settingsManager.CurrentValue.HistoryService.Cqrs;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory
            {
                Uri = _settings.RabbitConnString
            };
            var rabbitMqEndpoint = rabbitMqSettings.Endpoint.ToString();

            builder.RegisterType<CashInProjection>();
            builder.RegisterType<CashOutProjection>();
            builder.RegisterType<CashTransferProjection>();
            builder.RegisterType<ExecutionProjection>();

            builder.Register(ctx =>
            {
                var logFactory = ctx.Resolve<ILogFactory>();
                var messagingEngine = new MessagingEngine(
                    logFactory,
                    new TransportResolver(
                        new Dictionary<string, TransportInfo>
                        {
                                {
                                    "RabbitMq",
                                    new TransportInfo(
                                        rabbitMqEndpoint,
                                        rabbitMqSettings.UserName,
                                        rabbitMqSettings.Password, "None", "RabbitMq")
                                }
                        }),
                    new RabbitMqTransportFactory(logFactory));
                return CreateEngine(ctx, messagingEngine, logFactory);
            })
                .As<ICqrsEngine>()
                .AutoActivate()
                .SingleInstance();
        }

        private CqrsEngine CreateEngine(
            IComponentContext ctx,
            IMessagingEngine messagingEngine,
            ILogFactory logFactory)
        {
            const string boundedContext = "history";
            const string defaultRoute = "self";

            return new CqrsEngine(
                logFactory,
                ctx.Resolve<IDependencyResolver>(),
                messagingEngine,
                new DefaultEndpointProvider(),
                true,
                Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver(
                    "RabbitMq",
                    Messaging.Serialization.SerializationFormat.ProtoBuf,
                    environment: "lykke")),

                Register.BoundedContext(boundedContext)
                    .ListeningEvents(typeof(CashInProcessedEvent))
                    .From(PostProcessingBoundedContext.Name)
                    .On(defaultRoute)
                    .WithProjection(typeof(CashInProjection), PostProcessingBoundedContext.Name)

                    .ListeningEvents(typeof(CashOutProcessedEvent))
                    .From(PostProcessingBoundedContext.Name)
                    .On(defaultRoute)
                    .WithProjection(typeof(CashOutProjection), PostProcessingBoundedContext.Name)

                    .ListeningEvents(typeof(CashTransferProcessedEvent))
                    .From(PostProcessingBoundedContext.Name)
                    .On(defaultRoute)
                    .WithProjection(typeof(CashTransferProjection), PostProcessingBoundedContext.Name)

                    .ListeningEvents(typeof(ExecutionProcessedEvent))
                    .From(PostProcessingBoundedContext.Name)
                    .On(defaultRoute)
                    .WithProjection(typeof(ExecutionProjection), PostProcessingBoundedContext.Name));
        }
    }
}
