using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.History.Contracts.Cqrs.Commands;
using Lykke.Service.History.Core.Domain;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;

namespace Lykke.Service.History.Workflow.Handlers
{
    public class CashoutCommandHandler
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly ILog _logger;

        public CashoutCommandHandler(IHistoryRecordsRepository historyRecordsRepository, ILogFactory logFactory)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _logger = logFactory.CreateLog(this);
        }

        public async Task<CommandHandlingResult> Handle(SaveCashoutCommand command)
        {
            var entity = Mapper.Map<Cashout>(command);

            if (!await _historyRecordsRepository.TryInsertAsync(entity))
            {
                _logger.Warning($"Skipped duplicated cashout record", context: new { id = command.Id });
            }

            return CommandHandlingResult.Ok();
        }
    }
}
