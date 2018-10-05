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
    public class CashInProjection
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly ILog _logger;
        private readonly IOperationsRepository _operationsRepository;

        public CashInProjection(IHistoryRecordsRepository historyRecordsRepository, ILogFactory logFactory, IOperationsRepository operationsRepository)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _operationsRepository = operationsRepository;
            _logger = logFactory.CreateLog(this);
        }

        public async Task<CommandHandlingResult> Handle(CashInProcessedEvent @event)
        {
            var entity = Mapper.Map<Cashin>(@event);

            var operation = await _operationsRepository.Get(entity.Id);

            if (operation != null)
            {
                entity.OperationType = operation.Type;
            }

            if (!await _historyRecordsRepository.TryInsertAsync(entity))
                _logger.Warning($"Skipped duplicated cashin record", context: new
                {
                    id = @event.OperationId
                });

            return CommandHandlingResult.Ok();
        }
    }
}
