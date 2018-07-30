using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.History.Contracts.Cqrs.Commands;
using Lykke.Service.History.PostgresRepositories;
using Lykke.Service.History.PostgresRepositories.Entities;

namespace Lykke.Service.History.Workflow.Handlers
{
    public class CashinCommandHandler
    {
        private readonly HistoryRepository _historyRepository;
        private readonly ILog _logger;

        public CashinCommandHandler(HistoryRepository historyRepository, ILogFactory logFactory)
        {
            _historyRepository = historyRepository;
            _logger = logFactory.CreateLog(this);
        }

        public async Task<CommandHandlingResult> Handle(SaveCashinCommand command)
        {
            var entity = Mapper.Map<HistoryEntity>(command);

            entity.Type = Core.Domain.HistoryType.CashIn;
            entity.State = Core.Domain.HistoryState.Finished;

            if (!await _historyRepository.TryInsertAsync(entity))
            {
                _logger.Warning($"Duplicated cashin, {command.Id}");
            }

            return CommandHandlingResult.Ok();
        }
    }
}
