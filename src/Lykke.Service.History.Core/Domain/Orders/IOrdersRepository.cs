using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.History.Core.Domain.Orders
{
    public interface IOrdersRepository
    {
        Task<bool> InsertOrUpdateAsync(Order order);

        Task<Order> Get(Guid id);
    }
}
