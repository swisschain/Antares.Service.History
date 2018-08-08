using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.Orders;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.History.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersRepository _ordersRepository;

        public OrdersController(IOrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository;
        }

        [HttpGet("{id}")]
        public async Task<Order> GetOrder(Guid id)
        {
            return await _ordersRepository.Get(id);
        }

        [HttpGet("list")]
        public async Task<IEnumerable<Order>> GetOrders(
            [FromQuery]Guid walletId,
            [FromQuery(Name = "status")]OrderStatus[] status,
            int offset = 0,
            int limit = 100)
        {
            if (status.Length == 0)
                status = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().ToArray();

            return await _ordersRepository.GetOrders(walletId, status, offset, limit);
        }
    }
}
