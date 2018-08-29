using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.History.Contracts;
using Lykke.Service.History.Contracts.Enums;
using Lykke.Service.History.Contracts.History;
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
        /// <returns></returns>
        [Get("/api/history")]
        Task<IEnumerable<BaseHistoryModel>> GetHistoryByWalletAsync(Guid walletId,
            [Query(CollectionFormat.Multi)] HistoryType[] type = null,
            string assetId = null,
            string assetPairId = null,
            int offset = 0,
            int limit = 100);
    }
}
