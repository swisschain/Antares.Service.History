using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.History.Contracts.Enums;
using Lykke.Service.History.Contracts.History;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.History.Controllers
{
    [Route("api/trades")]
    [ApiController]
    public class TradesController
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IHistoryRecordsRepository _historyRecordsRepository;

        public TradesController(
            IOrdersRepository ordersRepository,
            IHistoryRecordsRepository historyRecordsRepository)
        {
            _ordersRepository = ordersRepository;
            _historyRecordsRepository = historyRecordsRepository;
        }
        
        /// <summary>
        /// Get wallet trades
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="assetId"></param>
        /// <param name="assetPairId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="tradeType"></param>
        /// <returns></returns>
        [HttpGet("")]
        [SwaggerOperation("GetTrades")]
        [ProducesResponseType(typeof(IReadOnlyList<TradeModel>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<TradeModel>> GetTrades(
            [FromQuery] Guid walletId,
            [FromQuery] string assetId = null,
            [FromQuery] string assetPairId = null,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 100,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery(Name = "tradeType")] TradeType? tradeType = null)
        {
            bool? onlyBuyTrades = null;
            if (tradeType == TradeType.Buy)
                onlyBuyTrades = true;
            if (tradeType == TradeType.Sell)
                onlyBuyTrades = false;

            var data = await _historyRecordsRepository.GetTradesByWallet(walletId, offset, limit, assetPairId, assetId, from, to, onlyBuyTrades);

            return Mapper.Map<IReadOnlyList<TradeModel>>(data);
        }
        
        /// <summary>
        /// Get order trades
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("order/{walletId}/{id}")]
        [SwaggerOperation("GetOrderTrades")]
        [ProducesResponseType(typeof(IReadOnlyList<TradeModel>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<TradeModel>> GetTradesByOrderId(Guid walletId, Guid id)
        {
            var trades = await _ordersRepository.GetTradesByOrderId(walletId, id);
            return Mapper.Map<IReadOnlyList<TradeModel>>(trades);
        }
    }
}
