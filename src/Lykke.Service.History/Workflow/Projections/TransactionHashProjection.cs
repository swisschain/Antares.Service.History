using System;
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
        private readonly string _crossClientTransactionHashSubstituition = "0x";

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
                throw new InvalidOperationException($"Transaction hash was not set. " +
                                                    $"OperationId: {@event.OperationId}, TxHash: {@event.TxHash}");

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
                throw new InvalidOperationException($"Transaction hash was not set. " +
                                                    $"OperationId: {@event.OperationId}, TxHash: {@event.TxHash}");

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
                throw new InvalidOperationException($"Transaction hash was not set. " +
                                                    $"OperationId: {@event.OperationId}, TxHash: {@event.TransactionHash}");

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
                throw new InvalidOperationException($"Transaction hash was not set. " +
                                                    $"OperationId: {@event.OperationId}, TxHash: {@event.TransactionHash}");

            return CommandHandlingResult.Ok();
        }

        /// <summary>
        ///     BIL batched cashout event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(
            Job.BlockchainCashoutProcessor.Contract.Events.CashoutsBatchCompletedEvent @event)
        {
            if (@event.Cashouts == null || @event.Cashouts.Length == 0)
            {
               throw new NotImplementedException($"BIL batched cashout event, BatchId {@event.BatchId}");
            }

            foreach (var cashout in @event.Cashouts)
            {
                if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(cashout.OperationId, @event.TransactionHash))
                    throw new InvalidOperationException($"Transaction hash was not set. " +
                                                        $"OperationId: {cashout.OperationId}, TxHash: {@event.TransactionHash}");
            }

            return CommandHandlingResult.Ok();
        }

        /// <summary>
        ///     BIL cross client cashout event completed
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(
            Job.BlockchainCashoutProcessor.Contract.Events.CrossClientCashoutCompletedEvent @event)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(@event.OperationId,
                _crossClientTransactionHashSubstituition))
                throw new InvalidOperationException($"Transaction hash was not set. " +
                                                    $"OperationId: {@event.OperationId}, TxHash: {_crossClientTransactionHashSubstituition}");

            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(@event.CashinOperationId,
                _crossClientTransactionHashSubstituition))
                throw new InvalidOperationException($"Transaction hash was not set. " +
                                                    $"OperationId: {@event.OperationId}, TxHash: {_crossClientTransactionHashSubstituition}");

            return CommandHandlingResult.Ok();
        }
    }
}
