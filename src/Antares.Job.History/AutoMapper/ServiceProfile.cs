using System;
using System.Collections.Generic;
using Antares.Job.History.RabbitSubscribers.Events;
using Antares.Service.History.Contracts.Cqrs.Commands;
using Antares.Service.History.Contracts.History;
using Antares.Service.History.Core;
using Antares.Service.History.Core.Domain.Enums;
using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.Core.Domain.Orders;
using AutoMapper;
using OrderModel = Antares.Job.History.RabbitSubscribers.Models.OrderModel;
using TradeModel = Antares.Job.History.RabbitSubscribers.Models.TradeModel;

namespace Antares.Job.History.AutoMapper
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<TradeModel, Trade>();

            CreateMap<OrderModel, Order>();

            CreateMap<ExecutionProcessedEvent, IEnumerable<Order>>().ConvertUsing<ExecutionConverter>();

            CreateMap<OrderPlacedEvent, OrderEvent>()
                .ForMember(x => x.Status, o => o.UseValue(OrderStatus.Placed))
                .ForMember(x => x.Id, o => o.MapFrom(s => Utils.IncrementGuid(s.OrderId, (int)OrderStatus.Placed)))
                .ForMember(x => x.Timestamp, o => o.MapFrom(s => s.CreateDt));

            CreateMap<OrderCancelledEvent, OrderEvent>()
                .ForMember(x => x.Status, o => o.UseValue(OrderStatus.Cancelled))
                .ForMember(x => x.Id, o => o.MapFrom(s => Utils.IncrementGuid(s.OrderId, (int)OrderStatus.Cancelled)));

            CreateMap<CreateForwardCashinCommand, Cashin>()
                .ForMember(x => x.Id, o => o.MapFrom(s => s.OperationId))
                .ForMember(x => x.State, o => o.UseValue(HistoryState.Finished))
                .ForMember(x => x.Volume, o => o.MapFrom(s => Math.Abs(s.Volume)));

            CreateMap<BaseHistoryRecord, BaseHistoryModel>();

            CreateMap<Cashin, CashinModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();
            CreateMap<Cashout, CashoutModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();
            CreateMap<Trade, Antares.Service.History.Contracts.History.TradeModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();
            CreateMap<OrderEvent, OrderEventModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();

            CreateMap<Order, OrderModel>();
        }
    }
}
