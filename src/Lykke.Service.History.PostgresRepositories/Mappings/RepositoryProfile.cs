using AutoMapper;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.History.PostgresRepositories.Entities;

namespace Lykke.Service.History.PostgresRepositories.Mappings
{
    public class RepositoryProfile : Profile
    {
        public RepositoryProfile()
        {
            CreateMap<HistoryEntity, Cashin>()
                .ForMember(x => x.BlockchainHash, o => o.MapFrom(p => p.ContextObject.BlockchainHash))
                .ForMember(x => x.State, o => o.MapFrom(p => p.ContextObject.State))
                .ForMember(x => x.FeeSize, o => o.MapFrom(p => p.ContextObject.FeeSize))
                .ReverseMap()
                .ForPath(x => x.ContextObject.BlockchainHash, o => o.MapFrom(p => p.BlockchainHash))
                .ForPath(x => x.ContextObject.State, o => o.MapFrom(p => p.State))
                .ForPath(x => x.ContextObject.FeeSize, o => o.MapFrom(p => p.FeeSize))
                .ForMember(x => x.Type, o => o.UseValue(HistoryType.CashIn));

            CreateMap<HistoryEntity, Cashout>()
                .ForMember(x => x.BlockchainHash, o => o.MapFrom(p => p.ContextObject.BlockchainHash))
                .ForMember(x => x.State, o => o.MapFrom(p => p.ContextObject.State))
                .ForMember(x => x.FeeSize, o => o.MapFrom(p => p.ContextObject.FeeSize))
                .ReverseMap()
                .ForPath(x => x.ContextObject.BlockchainHash, o => o.MapFrom(p => p.BlockchainHash))
                .ForPath(x => x.ContextObject.State, o => o.MapFrom(p => p.State))
                .ForPath(x => x.ContextObject.FeeSize, o => o.MapFrom(p => p.FeeSize))
                .ForMember(x => x.Type, o => o.UseValue(HistoryType.CashOut));

            CreateMap<HistoryEntity, Transfer>()
                .ForMember(x => x.FeeSize, o => o.MapFrom(p => p.ContextObject.FeeSize))
                .ReverseMap()
                .ForPath(x => x.ContextObject.FeeSize, o => o.MapFrom(p => p.FeeSize))
                .ForMember(x => x.Type, o => o.UseValue(HistoryType.Transfer));

            CreateMap<HistoryEntity, Trade>()
                .ForMember(x => x.Price, o => o.MapFrom(p => p.ContextObject.Price))
                .ForMember(x => x.FeeSize, o => o.MapFrom(p => p.ContextObject.FeeSize))
                .ForMember(x => x.FeeAssetId, o => o.MapFrom(p => p.ContextObject.FeeAssetId))
                .ForMember(x => x.Index, o => o.MapFrom(p => p.ContextObject.TradeIndex))
                .ForMember(x => x.Role, o => o.MapFrom(p => p.ContextObject.TradeRole))
                .ForMember(x => x.OppositeAssetId, o => o.MapFrom(p => p.ContextObject.TradeOppositeAssetId))
                .ForMember(x => x.OppositeVolume, o => o.MapFrom(p => p.ContextObject.TradeOppositeVolume))
                .ForMember(x => x.OrderId, o => o.MapFrom(p => p.ContextObject.OrderId))
                .ReverseMap()
                .ForPath(x => x.ContextObject.Price, o => o.MapFrom(p => p.Price))
                .ForPath(x => x.ContextObject.FeeSize, o => o.MapFrom(p => p.FeeSize))
                .ForPath(x => x.ContextObject.FeeAssetId, o => o.MapFrom(p => p.FeeAssetId))
                .ForPath(x => x.ContextObject.TradeIndex, o => o.MapFrom(p => p.Index))
                .ForPath(x => x.ContextObject.TradeRole, o => o.MapFrom(p => p.Role))
                .ForPath(x => x.ContextObject.TradeOppositeAssetId, o => o.MapFrom(p => p.OppositeAssetId))
                .ForPath(x => x.ContextObject.TradeOppositeVolume, o => o.MapFrom(p => p.OppositeVolume))
                .ForPath(x => x.ContextObject.OrderId, o => o.MapFrom(p => p.OrderId))
                .ForMember(x => x.Type, o => o.UseValue(HistoryType.Trade));

            CreateMap<HistoryEntity, OrderEvent>()
                .ForMember(x => x.Price, o => o.MapFrom(p => p.ContextObject.Price))
                .ForMember(x => x.OrderId, o => o.MapFrom(p => p.ContextObject.OrderId))
                .ForMember(x => x.Status, o => o.MapFrom(p => p.ContextObject.OrderEventStatus))
                .ReverseMap()
                .ForPath(x => x.ContextObject.Price, o => o.MapFrom(p => p.Price))
                .ForPath(x => x.ContextObject.OrderId, o => o.MapFrom(p => p.OrderId))
                .ForPath(x => x.ContextObject.OrderEventStatus, o => o.MapFrom(p => p.Status))
                .ForMember(x => x.Type, o => o.UseValue(HistoryType.OrderEvent));

            CreateMap<OrderEntity, Order>();
        }
    }
}
