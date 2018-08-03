using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.History.Contracts.Cqrs.Commands;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using MoreLinq;

namespace Lykke.Service.History.Workflow.Handlers
{
    public class ExecutionCommandHandler
    {
        private const int BulkSize = 5000;

        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly IOrdersRepository _ordersRepository;
        private readonly ILog _logger;

        public ExecutionCommandHandler(IHistoryRecordsRepository historyRecordsRepository, ILogFactory logFactory, IOrdersRepository ordersRepository)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _ordersRepository = ordersRepository;
            _logger = logFactory.CreateLog(this);
        }

        public async Task<CommandHandlingResult> Handle(SaveExecutionCommand command)
        {
            var orders = Mapper.Map<IEnumerable<Order>>(command).ToList();

            foreach (var order in orders)
            {
                var result = await _ordersRepository.InsertOrUpdateAsync(order);
                if (!result)
                    _logger.Warning($"Order {order.Id} was not updated, sequence {command.SequenceNumber}");
            }

            var trades = orders.SelectMany(x => x.Trades);

            var batched = trades.Batch(BulkSize).ToList();
            for (var i = 0; i < batched.Count; i++)
            {
                var result = await _historyRecordsRepository.TryInsertBulkAsync(batched[i]);
                if (!result)
                    _logger.Warning($"Bulk was not inserted, sequence {command.SequenceNumber}, bulk: {i}");
            }

            return CommandHandlingResult.Ok();
        }
    }
}
