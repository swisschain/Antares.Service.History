using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.History.Contracts.Enums;
using Lykke.Service.History.Contracts.History;
using Lykke.Service.History.Core.Domain.History;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using HistoryType = Lykke.Service.History.Core.Domain.Enums.HistoryType;

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

        /// <summary>
        /// Get wallet history
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="type"></param>
        /// <param name="assetId"></param>
        /// <param name="assetPairId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="fromDt"></param>
        /// <param name="toDt"></param>
        /// <returns></returns>
        [HttpGet("")]
        [SwaggerOperation("GetHistory")]
        [ProducesResponseType(typeof(IReadOnlyList<BaseHistoryModel>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<BaseHistoryModel>> GetHistory(
            [FromQuery] Guid walletId,
            [FromQuery(Name = "type")] HistoryType[] type,
            [FromQuery] string assetId = null,
            [FromQuery] string assetPairId = null,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 100,
            [FromQuery] DateTime? fromDt = null,
            [FromQuery] DateTime? toDt = null)
        {
            if (type.Length == 0)
                type = Enum.GetValues(typeof(HistoryType)).Cast<HistoryType>().ToArray();

            var data = await _historyRecordsRepository.GetByWallet(walletId, type, offset, limit, assetPairId, assetId, fromDt, toDt);

            return Mapper.Map<IReadOnlyList<BaseHistoryModel>>(data);
        }
        
        /// <summary>
        /// Get wallet history
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{walletId}/{id}")]
        [SwaggerOperation("GetHistoryItem")]
        [ProducesResponseType(typeof(BaseHistoryModel), (int)HttpStatusCode.OK)]
        public async Task<BaseHistoryModel> GetHistory(Guid walletId, Guid id)
        {
            var data = await _historyRecordsRepository.Get(id, walletId);

            return Mapper.Map<BaseHistoryModel>(data);
        }

        /// <summary>
        /// Get wallet trades
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="assetId"></param>
        /// <param name="assetPairId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="fromDt"></param>
        /// <param name="toDt"></param>
        /// <param name="tradeType"></param>
        /// <returns></returns>
        [HttpGet("trades")]
        [SwaggerOperation("GetTrades")]
        [ProducesResponseType(typeof(IReadOnlyList<TradeModel>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<TradeModel>> GetTrades(
            [FromQuery] Guid walletId,
            [FromQuery] string assetId = null,
            [FromQuery] string assetPairId = null,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 100,
            [FromQuery] DateTime? fromDt = null,
            [FromQuery] DateTime? toDt = null,
            [FromQuery(Name = "type")] TradeType? tradeType = null)
        {
            bool? onlyBuyTrades = null;
            if (tradeType == TradeType.Buy)
                onlyBuyTrades = true;
            if (tradeType == TradeType.Sell)
                onlyBuyTrades = false;

            var data = await _historyRecordsRepository.GetTradesByWallet(walletId, offset, limit, assetPairId, assetId, fromDt, toDt, onlyBuyTrades);

            return Mapper.Map<IReadOnlyList<TradeModel>>(data);
        }
    }
}
