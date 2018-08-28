﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.PostgresRepositories.Entities;
using Lykke.Service.History.PostgresRepositories.JsonbQuery;
using Lykke.Service.History.PostgresRepositories.Mappings;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PostgreSQLCopyHelper;

namespace Lykke.Service.History.PostgresRepositories.Repositories
{
    public class HistoryRecordsRepository : IHistoryRecordsRepository
    {
        private const string DuplicateSqlState = "23505";
        private static readonly PostgreSQLCopyHelper<HistoryEntity> BulkMapping;
        private readonly ConnectionFactory _connectionFactory;

        private readonly string _insertQuery = $@"
insert into {Constants.HistoryTableName}(id, wallet_id, asset_id, assetpair_id, volume, type, create_dt, context)
    values (@Id, @WalletId, @AssetId, @AssetPairId, @Volume, @Type, @Timestamp, @Context::jsonb)
ON CONFLICT (id, wallet_id) DO NOTHING;
";

        private readonly string _updateBlockchainHashQuery = $@"
update {Constants.HistoryTableName}
set context = jsonb_set(coalesce(context, '{{}}'), '{{{
                nameof(HistoryEntityContext.BlockchainHash)
            }}}', coalesce(to_jsonb(@Hash::text), jsonb 'null'))
where id = @Id
";

        static HistoryRecordsRepository()
        {
            BulkMapping = HistoryEntityBulkMapping.Generate();
        }

        public HistoryRecordsRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<BaseHistoryRecord> Get(Guid id, Guid walletId)
        {
            using (var connection = _connectionFactory.CreateDataContext())
            {
                return HistoryTypeMapper.Map(
                    await connection.History.FirstOrDefaultAsync(x => x.Id == id && x.WalletId == walletId));
            }
        }

        public async Task<bool> InsertBulkAsync(IEnumerable<BaseHistoryRecord> records)
        {
            using (var connection = await _connectionFactory.CreateNpgsqlConnection())
            {
                var baseHistoryRecords = records.ToArray();

                try
                {
                    BulkMapping.SaveAll(connection, baseHistoryRecords.Select(HistoryTypeMapper.Map));
                }
                catch (PostgresException e) when (e.SqlState == DuplicateSqlState)
                {
                    // fallback, try to insert one by one
                    foreach (var trade in baseHistoryRecords)
                        await TryInsertAsync(trade);
                }
            }

            return true;
        }

        public async Task<bool> TryInsertAsync(BaseHistoryRecord entity)
        {
            using (var connection = await _connectionFactory.CreateNpgsqlConnection())
            {
                var result = await connection.ExecuteAsync(_insertQuery, HistoryTypeMapper.Map(entity));

                return result > 0;
            }
        }

        public async Task<bool> UpdateBlockchainHashAsync(Guid id, string hash)
        {
            using (var connection = await _connectionFactory.CreateNpgsqlConnection())
            {
                var result = await connection.ExecuteAsync(_updateBlockchainHashQuery, new
                {
                    Id = id,
                    Hash = hash
                });

                return result > 0;
            }
        }

        public async Task<IEnumerable<BaseHistoryRecord>> GetByWallet(Guid walletId, HistoryType[] type, int offset,
            int limit, string assetPairId = null, string assetId = null)
        {
            using (var context = _connectionFactory.CreateDataContext())
            {
                var query = context.History
                    .Where(x => x.WalletId == walletId && type.Contains(x.Type))
                    .Where(x => string.IsNullOrWhiteSpace(assetPairId) || x.AssetPairId == assetPairId)
                    .Where(x => string.IsNullOrWhiteSpace(assetId) || x.AssetId == assetId ||
                                (x.Type == HistoryType.Trade && x.Context.JsonbPath<string>(nameof(HistoryEntityContext.TradeQuotingAssetId)) == assetId))
                    .OrderByDescending(x => x.Timestamp)
                    .Skip(offset)
                    .Take(limit);

                return (await query.ToListAsync()).Select(HistoryTypeMapper.Map);
            }
        }
    }
}