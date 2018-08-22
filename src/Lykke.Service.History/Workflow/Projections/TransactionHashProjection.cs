using System;
using System.Threading.Tasks;
using Common.Log;
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
        /// Bitcoin cashin event
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(Bitcoin.Contracts.Events.CashinCompletedEvent command)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(command.OperationId, command.TxHash))
            {
                _logger.Warning($"Transaction hash was not set", context: new { id = command.OperationId, Hash = command.TxHash });

                return CommandHandlingResult.Fail(TimeSpan.FromSeconds(10));
            }

            return CommandHandlingResult.Ok();
        }

        /// <summary>
        /// Bitcoin cashout event
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(Bitcoin.Contracts.Events.CashoutCompletedEvent command)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(command.OperationId, command.TxHash))
            {
                _logger.Warning($"Transaction hash was not set", context: new { id = command.OperationId, Hash = command.TxHash });

                return CommandHandlingResult.Fail(TimeSpan.FromSeconds(10));
            }
            
            return CommandHandlingResult.Ok();
        }

        /// <summary>
        /// BIL cashin event
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(Job.BlockchainCashinDetector.Contract.Events.CashinCompletedEvent command)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(command.OperationId, command.TransactionHash))
            {
                _logger.Warning($"Transaction hash was not set", context: new { id = command.OperationId, Hash = command.TransactionHash });

                return CommandHandlingResult.Fail(TimeSpan.FromSeconds(10));
            }

            return CommandHandlingResult.Ok();
        }

        /// <summary>
        /// BIL cashout event
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(Job.BlockchainCashoutProcessor.Contract.Events.CashoutCompletedEvent command)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(command.OperationId, command.TransactionHash))
            {
                _logger.Warning($"Transaction hash was not set", context: new { id = command.OperationId, Hash = command.TransactionHash });

                return CommandHandlingResult.Fail(TimeSpan.FromSeconds(10));
            }

            return CommandHandlingResult.Ok();
        }
    }
}
