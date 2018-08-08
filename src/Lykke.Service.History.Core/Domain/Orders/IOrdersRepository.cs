using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.History.Core.Domain.Enums;

namespace Lykke.Service.History.Core.Domain.Orders
{
    public interface IOrdersRepository
    {
        Task UpsertBulkAsync(IEnumerable<Order> records);

        Task<bool> InsertOrUpdateAsync(Order order);

        Task<Order> Get(Guid id);

        Task<IEnumerable<Order>> GetOrders(Guid walletId, OrderStatus[] statuses, int offset, int limit);
    }
}
