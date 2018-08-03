using System;
using System.Threading.Tasks;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.History.Controllers
{
    [Route("api/history")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;

        public HistoryController(IHistoryRecordsRepository historyRecordsRepository)
        {
            _historyRecordsRepository = historyRecordsRepository;
        }

        [HttpGet("")]
        public async Task<object> GetCommonHistory(
            [FromQuery] Guid walletId,
            [FromQuery(Name = "type")] HistoryType[] type,
            [FromQuery] string assetId = null,
            [FromQuery] string assetPairId = null,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 100)
        {
            var data = await _historyRecordsRepository.GetByWallet(walletId, type, offset, limit, assetPairId, assetId);

            return data;
        }
    }
}
