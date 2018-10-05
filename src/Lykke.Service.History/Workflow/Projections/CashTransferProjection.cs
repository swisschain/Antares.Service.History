using System.Collections.Generic;
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
    public class CashTransferProjection
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly ILog _logger;
        private readonly IOperationsRepository _operationsRepository;

        public CashTransferProjection(IHistoryRecordsRepository historyRecordsRepository, ILogFactory logFactory, IOperationsRepository operationsRepository)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _operationsRepository = operationsRepository;
            _logger = logFactory.CreateLog(this);
        }

        public async Task<CommandHandlingResult> Handle(CashTransferProcessedEvent @event)
        {
            var cashInOuts = Mapper.Map<IEnumerable<BaseHistoryRecord>>(@event);


            foreach (var cashInOut in cashInOuts)
            {
                var operation = await _operationsRepository.Get(cashInOut.Id);

                if (cashInOut is Cashin cashin)
                {
                    if (operation != null)
                        cashin.OperationType = operation.Type;

                    if (!await _historyRecordsRepository.TryInsertAsync(cashin))
                        _logger.Warning($"Skipped duplicated cashin record", context: new
                        {
                            id = @event.OperationId
                        });
                }

                if (cashInOut is Cashout cashout)
                {
                    if (operation != null)
                        cashout.OperationType = operation.Type;

                    if (!await _historyRecordsRepository.TryInsertAsync(cashout))
                        _logger.Warning($"Skipped duplicated cashout record", context: new
                        {
                            id = @event.OperationId
                        });
                }
            }

            return CommandHandlingResult.Ok();
        }
    }
}
