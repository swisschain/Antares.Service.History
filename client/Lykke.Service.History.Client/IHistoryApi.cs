using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.History.Contracts.Enums;
using Lykke.Service.History.Contracts.History;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Lykke.Service.History.Client
{
    /// <summary>
    ///     History client API interface.
    /// </summary>
    [PublicAPI]
    public interface IHistoryApi
    {
        /// <summary>
        /// Get history by wallet id
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
        [Get("/api/history")]
        Task<IEnumerable<BaseHistoryModel>> GetHistoryByWalletAsync(Guid walletId,
            [Query(CollectionFormat.Multi)] HistoryType[] type = null,
            string assetId = null,
            string assetPairId = null,
            int offset = 0,
            int limit = 100,
            DateTime? fromDt = null,
            DateTime? toDt = null);

        /// <summary>
        /// Get history item by id
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Get("/api/history/{walletId}/{id}")]
        Task<BaseHistoryModel> GetHistoryItemByIdAsync(Guid walletId, Guid id);

        /// <summary>
        /// Get trades by wallet id
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
        [Get("/api/history/trades")]
        Task<IEnumerable<TradeModel>> GetTradesByWalletAsync(
            Guid walletId,
            string assetId = null,
            string assetPairId = null,
            int offset = 0,
            int limit = 100,
            DateTime? fromDt = null,
            DateTime? toDt = null,
            TradeType? tradeType = null);
    }
}
