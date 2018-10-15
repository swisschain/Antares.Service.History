using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;

namespace Lykke.Service.History.Core.Domain.Orders
{
    public interface IOrdersRepository
    {
        Task UpsertBulkAsync(IEnumerable<Order> records);

        Task<bool> InsertOrUpdateAsync(Order order);

        Task<Order> Get(Guid id);

        Task<IEnumerable<Order>> GetOrders(Guid walletId, OrderType[] types, OrderStatus[] statuses, string assetPairId, int offset,
            int limit);
        
        Task<IEnumerable<Trade>> GetTradesByOrderId(Guid walletId, Guid id);
    }
}
