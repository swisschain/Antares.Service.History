using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;

namespace Lykke.Service.History.Workflow.Projections
{
    public class OrderEventProjection
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly ILog _logger;

        public OrderEventProjection(IHistoryRecordsRepository historyRecordsRepository, ILogFactory logFactory)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _logger = logFactory.CreateLog(this);
        }

        public async Task<CommandHandlingResult> Handle(OrderPlacedEvent @event)
        {
            var entity = Mapper.Map<OrderEvent>(@event);

            if (!await _historyRecordsRepository.TryInsertAsync(entity))
            {
                _logger.Warning($"Skipped duplicated cashin record", context: new { id = entity.Id, orderId = @event.OrderId, type = "placed" });
            }

            return CommandHandlingResult.Ok();
        }

        public async Task<CommandHandlingResult> Handle(OrderCancelledEvent @event)
        {
            var entity = Mapper.Map<OrderEvent>(@event);

            if (!await _historyRecordsRepository.TryInsertAsync(entity))
            {
                _logger.Warning($"Skipped duplicated cashin record", context: new { id = entity.Id, orderId = @event.OrderId, type = "cancelled" });
            }

            return CommandHandlingResult.Ok();
        }
    }
}
