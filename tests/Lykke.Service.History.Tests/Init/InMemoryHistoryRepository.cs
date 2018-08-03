using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.History.Core.Domain;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;

namespace Lykke.Service.History.Tests.Init
{
    public class InMemoryHistoryRepository : IHistoryRecordsRepository
    {
        private readonly List<BaseHistoryRecord> _data = new List<BaseHistoryRecord>();

        public Task<BaseHistoryRecord> Get(Guid id, Guid walletId)
        {
            return Task.FromResult(_data.FirstOrDefault(x => x.Id == id && x.WalletId == walletId));
        }

        public Task<bool> TryInsertBulkAsync(IEnumerable<BaseHistoryRecord> records)
        {
            if (_data.Any(x => records.Any(o => o.Id == x.Id)))
                Task.FromResult(false);

            _data.AddRange(records);

            return Task.FromResult(true);
        }

        public async Task<bool> TryInsertAsync(BaseHistoryRecord entity)
        {
            if (await Get(entity.Id, entity.WalletId) == null)
            {
                _data.Add(entity);
                return true;
            }

            return false;
        }

        public Task<bool> UpdateBlockchainHashAsync(Guid id, string hash)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BaseHistoryRecord>> GetByWallet(Guid walletId, HistoryType[] type, int offset, int limit, string assetpairId = null,
            string assetId = null)
        {
            var typesMap = new Dictionary<HistoryType, Type>()
            {
                {HistoryType.CashIn, typeof(Cashin)},
                {HistoryType.CashOut, typeof(Cashout)},
                {HistoryType.Transfer, typeof(Transfer)},
                {HistoryType.Trade, typeof(Trade)},
                {HistoryType.OrderEvent, typeof(OrderEvent)}
            };

            var neededTypes = type.Select(x => typesMap[x]);

            return Task.FromResult(_data.Where(x => x.WalletId == walletId && neededTypes.Contains(x.GetType()))
                .Where(x => string.IsNullOrWhiteSpace(assetpairId) || ((dynamic)x).AssetPairId == assetpairId)
                .Where(x => string.IsNullOrWhiteSpace(assetId) || ((dynamic)x).AssetId == assetId || ((dynamic)x).OppositeAssetId == assetId)
                .OrderByDescending(x => x.Timestamp)
                .Skip(offset)
                .Take(limit));
        }
    }
}

