using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Operations;
using Lykke.Service.Operations.Contracts.Events;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;

namespace Lykke.Service.History.Workflow.Projections
{
    public class OperationsProjection
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly IOperationsRepository _operationsRepository;

        public OperationsProjection(
            IHistoryRecordsRepository historyRecordsRepository,
            IOperationsRepository operationsRepository)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _operationsRepository = operationsRepository;
        }

        public async Task<CommandHandlingResult> Handle(OperationCreatedEvent @event)
        {
            var operation = Mapper.Map<Operation>(@event);

            await _operationsRepository.TryInsertAsync(operation);

            await _historyRecordsRepository.UpdateOperationTypeAsync(operation.Id, operation.Type);

            return CommandHandlingResult.Ok();
        }

        public async Task<CommandHandlingResult> Handle(OperationCompletedEvent @event)
        {
            await _historyRecordsRepository.UpdateStateAsync(@event.OperationId, Core.Domain.Enums.HistoryState.Finished);

            return CommandHandlingResult.Ok();
        }
    }
}
