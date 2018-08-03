using System.Collections.Generic;
using AutoMapper;
using Lykke.Service.History.Contracts.Cqrs.Commands;
using Lykke.Service.History.Core.Domain;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;

namespace Lykke.Service.History.AutoMapper
{
    public class ExecutionConverter : ITypeConverter<SaveExecutionCommand, IEnumerable<Order>>
    {
        public IEnumerable<Order> Convert(SaveExecutionCommand source, IEnumerable<Order> destination, ResolutionContext context)
        {
            foreach (var item in source.Orders)
            {
                var order = Mapper.Map<Order>(item);

                order.SequenceNumber = source.SequenceNumber;

                foreach (var trade in order.Trades)
                {
                    trade.OrderId = order.Id;
                }

                yield return order;
            }
        }
    }
}
