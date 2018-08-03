using System;
using System.Collections.Generic;
using AutoMapper;
using Lykke.Service.History.Contracts.Cqrs.Commands;
using Lykke.Service.History.Contracts.Cqrs.Commands.Models;
using Lykke.Service.History.Contracts.Cqrs.Models;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;

namespace Lykke.Service.History.AutoMapper
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<SaveCashinCommand, Cashin>()
                .ForMember(x => x.State, o => o.UseValue(HistoryState.Finished))
                .ForMember(x => x.Volume, o => o.MapFrom(s => Math.Abs(s.Volume)));

            CreateMap<SaveCashoutCommand, Cashout>()
                .ForMember(x => x.State, o => o.UseValue(HistoryState.Finished))
                .ForMember(x => x.Volume, o => o.MapFrom(s => -Math.Abs(s.Volume)));

            CreateMap<SaveTransferCommand, IEnumerable<Transfer>>().ConvertUsing<TransferConverter>();

            CreateMap<TradeModel, Trade>();

            CreateMap<OrderModel, Order>();

            CreateMap<SaveExecutionCommand, IEnumerable<Order>>().ConvertUsing<ExecutionConverter>();
        }
    }
}
