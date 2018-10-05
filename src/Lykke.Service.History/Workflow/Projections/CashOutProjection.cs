using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Operations;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;

namespace Lykke.Service.History.Workflow.Projections
{
    public class CashOutProjection
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly ILog _logger;
        private readonly IOperationsRepository _operationsRepository;

        public CashOutProjection(IHistoryRecordsRepository historyRecordsRepository, ILogFactory logFactory, IOperationsRepository operationsRepository)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _operationsRepository = operationsRepository;
            _logger = logFactory.CreateLog(this);
        }

        public async Task<CommandHandlingResult> Handle(CashOutProcessedEvent @event)
        {
            var entity = Mapper.Map<Cashout>(@event);

            var operation = await _operationsRepository.Get(entity.Id);

            if (operation != null)
            {
                entity.OperationType = operation.Type;

                if (operation.Type == Core.Domain.Enums.HistoryOperationType.Crypto)
                    entity.State = Core.Domain.Enums.HistoryState.InProgress;
            }

            if (!await _historyRecordsRepository.TryInsertAsync(entity))
                _logger.Warning($"Skipped duplicated cashout record", context: new
                {
                    id = @event.OperationId
                });

            return CommandHandlingResult.Ok();
        }
    }
}
