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
        private readonly ILog _logger;

        public ExecutionProjection(IHistoryRecordsRepository historyRecordsRepository, ILogFactory logFactory, IOrdersRepository ordersRepository)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _ordersRepository = ordersRepository;
            _logger = logFactory.CreateLog(this);
        }

        public async Task<CommandHandlingResult> Handle(ExecutionProcessedEvent @event)
        {
            var sw = Stopwatch.StartNew();
            var builder = new StringBuilder();

            var orders = Mapper.Map<IEnumerable<Order>>(@event).ToList();

            builder.AppendLine($"Started {DateTime.UtcNow}, count: {orders.Count}");

            sw.Restart();

            await _ordersRepository.UpsertBulkAsync(orders);

            builder.AppendLine($"Orders: {sw.ElapsedMilliseconds}");
            sw.Restart();

            var trades = orders.SelectMany(x => x.Trades);

            var batched = trades.Batch(BulkSize).ToList();
            foreach (var tradesBatch in batched)
            {
                await _historyRecordsRepository.TryInsertBulkAsync(tradesBatch);
            }

            builder.AppendLine($"Trades: {sw.ElapsedMilliseconds}");
            sw.Restart();

            _logger.Info(builder.ToString());

            return CommandHandlingResult.Ok();
        }
    }
}
