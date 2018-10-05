using System;
using System.Collections.Generic;
using AutoMapper;
using Lykke.Service.History.Contracts.Cqrs.Commands;
using Lykke.Service.History.Contracts.History;
using Lykke.Service.History.Core;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Operations;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.Operations.Contracts;
using Lykke.Service.Operations.Contracts.Events;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Models;
using TradeModel = Lykke.Service.PostProcessing.Contracts.Cqrs.Models.TradeModel;

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

            CreateMap<CashTransferProcessedEvent, IEnumerable<BaseHistoryRecord>>().ConvertUsing<CashTransferConverter>();

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
            CreateMap<Trade, Contracts.History.TradeModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();
            CreateMap<OrderEvent, OrderEventModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();

            CreateMap<Order, OrderModel>();

            CreateMap<OperationCreatedEvent, Operation>()
                .ForMember(x => x.Id, o => o.MapFrom(t => t.Id))
                .ForMember(x => x.CreateDt, o => o.MapFrom(t => DateTime.UtcNow))
                .ForMember(x => x.Type, o => o.MapFrom(t => MapType(t.OperationType)));
        }

        private HistoryOperationType MapType(OperationType type)
        {
            switch (type)
            {
                case OperationType.Transfer:
                    return HistoryOperationType.Hft;
                case OperationType.VisaCardPayment:
                    return HistoryOperationType.Card;
                case OperationType.CashoutSwift:
                    return HistoryOperationType.Bank;
                case OperationType.Cashout:
                    return HistoryOperationType.Crypto;
                case OperationType.ForwardCashin:
                case OperationType.ForwardCashout:
                    return HistoryOperationType.Forward;
                default:
                    return HistoryOperationType.None;
            }
        }
    }
}
