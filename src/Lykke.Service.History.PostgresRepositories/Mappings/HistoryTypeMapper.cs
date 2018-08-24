using System;
using AutoMapper;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.PostgresRepositories.Entities;

namespace Lykke.Service.History.PostgresRepositories.Mappings
{
    internal class HistoryTypeMapper
    {
        internal static BaseHistoryRecord Map(HistoryEntity entity)
        {
            if (entity == null)
                return null;

            switch (entity.Type)
            {
                case HistoryType.CashIn:
                    return Mapper.Map<Cashin>(entity);
                case HistoryType.CashOut:
                    return Mapper.Map<Cashout>(entity);
                case HistoryType.Transfer:
                    return Mapper.Map<Transfer>(entity);
                case HistoryType.Trade:
                    return Mapper.Map<Trade>(entity);
                case HistoryType.OrderEvent:
                    return Mapper.Map<OrderEvent>(entity);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal static HistoryEntity Map(BaseHistoryRecord entity)
        {
            if (entity == null)
                return null;

            switch (entity)
            {
                case Cashin cashin:
                    return Mapper.Map<HistoryEntity>(cashin);
                case Cashout cashout:
                    return Mapper.Map<HistoryEntity>(cashout);
                case Transfer transfer:
                    return Mapper.Map<HistoryEntity>(transfer);
                case Trade trade:
                    return Mapper.Map<HistoryEntity>(trade);
                case OrderEvent orderEvent:
                    return Mapper.Map<HistoryEntity>(orderEvent);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
