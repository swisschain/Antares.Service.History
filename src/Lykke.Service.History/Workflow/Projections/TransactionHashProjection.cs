using System.Threading.Tasks;
using Common.Log;
using Lykke.Bitcoin.Contracts.Events;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.History.Core.Domain.History;

namespace Lykke.Service.History.Workflow.Projections
{
    public class TransactionHashProjection
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly ILog _logger;

        public TransactionHashProjection(IHistoryRecordsRepository historyRecordsRepository, ILogFactory logFactory)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _logger = logFactory.CreateLog(this);
        }

        /// <summary>
        ///     Bitcoin cashin event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(CashinCompletedEvent @event)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(@event.OperationId, @event.TxHash))
                _logger.Warning($"Transaction hash was not set, bitcoin cashin", context: new
                {
                    id = @event.OperationId,
                    hash = @event.TxHash
                });

            return CommandHandlingResult.Ok();
        }

        /// <summary>
        ///     Bitcoin cashout event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(CashoutCompletedEvent @event)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(@event.OperationId, @event.TxHash))
                _logger.Warning($"Transaction hash was not set, bitcoin cashout", context: new
                {
                    id = @event.OperationId,
                    hash = @event.TxHash
                });

            return CommandHandlingResult.Ok();
        }

        /// <summary>
        ///     BIL cashin event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(
            Job.BlockchainCashinDetector.Contract.Events.CashinCompletedEvent @event)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(@event.OperationId, @event.TransactionHash))
                _logger.Warning($"Transaction hash was not set, BIL cashin", context: new
                {
                    id = @event.OperationId,
                    hash = @event.TransactionHash
                });

            return CommandHandlingResult.Ok();
        }

        /// <summary>
        ///     BIL cashout event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(
            Job.BlockchainCashoutProcessor.Contract.Events.CashoutCompletedEvent @event)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(@event.OperationId, @event.TransactionHash))
                _logger.Warning($"Transaction hash was not set, BIL cashout", context: new
                {
                    id = @event.OperationId,
                    hash = @event.TransactionHash
                });

            return CommandHandlingResult.Ok();
        }
    }
}
