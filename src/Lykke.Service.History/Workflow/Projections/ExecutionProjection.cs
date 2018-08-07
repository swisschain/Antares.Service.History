using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
using MoreLinq;

namespace Lykke.Service.History.Workflow.Projections
{
    public class ExecutionProjection
    {
        private const int BulkSize = 5000;

        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly IOrdersRepository _ordersRepository;

        public ExecutionProjection(IHistoryRecordsRepository historyRecordsRepository, IOrdersRepository ordersRepository)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _ordersRepository = ordersRepository;
        }

        public async Task<CommandHandlingResult> Handle(ExecutionProcessedEvent @event)
        {
            var orders = Mapper.Map<IEnumerable<Order>>(@event).ToList();

            if (!orders.Any())
                return CommandHandlingResult.Ok();

            if (orders.Count > 1)
            {
                await _ordersRepository.UpsertBulkAsync(orders);
            }
            else
            {
                await _ordersRepository.InsertOrUpdateAsync(orders[0]);
            }

            var trades = orders.SelectMany(x => x.Trades);

            var batched = trades.Batch(BulkSize).ToList();
            foreach (var tradesBatch in batched)
            {
                await _historyRecordsRepository.TryInsertBulkAsync(tradesBatch);
            }

            return CommandHandlingResult.Ok();
        }
    }
}
