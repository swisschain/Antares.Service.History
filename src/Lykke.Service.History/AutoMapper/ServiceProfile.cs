using System;
using System.Collections.Generic;
using AutoMapper;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Models;

namespace Lykke.Service.History.AutoMapper
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<CashInProcessedEvent, Cashin>()
                .ForMember(x => x.Id, o => o.MapFrom(s => s.OperationId))
                .ForMember(x => x.State, o => o.UseValue(HistoryState.Finished))
                .ForMember(x => x.Volume, o => o.MapFrom(s => Math.Abs(s.Volume)));

            CreateMap<CashOutProcessedEvent, Cashout>()
                .ForMember(x => x.Id, o => o.MapFrom(s => s.OperationId))
                .ForMember(x => x.State, o => o.UseValue(HistoryState.Finished))
                .ForMember(x => x.Volume, o => o.MapFrom(s => -Math.Abs(s.Volume)));

            CreateMap<CashTransferProcessedEvent, IEnumerable<Transfer>>().ConvertUsing<TransferConverter>();

            CreateMap<TradeModel, Trade>();

            CreateMap<OrderModel, Order>();

            CreateMap<ExecutionProcessedEvent, IEnumerable<Order>>().ConvertUsing<ExecutionConverter>();
        }
    }
}
