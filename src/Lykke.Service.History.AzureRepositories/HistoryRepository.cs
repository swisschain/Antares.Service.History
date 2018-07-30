using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Dapper.FluentMap;
using Dapper.FluentMap.Dommel;
using Dommel;
using Lykke.Service.History.PostgresRepositories.Entities;
using Lykke.Service.History.PostgresRepositories.Mappings;
using PostgreSQLCopyHelper;

namespace Lykke.Service.History.PostgresRepositories
{
    public class HistoryRepository
    {
        private readonly NpgsqlConnectionProvider _npgsqlConnectionProvider;
        private readonly PostgreSQLCopyHelper<HistoryEntity> _historyBulkCopyHelper;
        private readonly string _tableName;

        public HistoryRepository(NpgsqlConnectionProvider npgsqlConnectionProvider, string tableName, string schemaName = "public")
        {
            _tableName = $"{tableName}.{schemaName}";

            _npgsqlConnectionProvider = npgsqlConnectionProvider;

            FluentMapper.Initialize(config =>
            {
                config.AddMap(new HistoryMap(tableName, schemaName));
                config.ForDommel();
            });

            _historyBulkCopyHelper = HistoryMap.GetBulkMapping(schemaName, tableName);
        }

        public async Task BulkCopy(IEnumerable<HistoryEntity> records)
        {
            using (var connection = await _npgsqlConnectionProvider.GetOpenedConnection())
            {
                _historyBulkCopyHelper.SaveAll(connection, records);
            }
        }

        public async Task<bool> TryInsertAsync(HistoryEntity entity)
        {
            using (var connection = await _npgsqlConnectionProvider.GetOpenedConnection())
            {
                var result = await connection.ExecuteAsync(string.Format(InsertQuery, _tableName), entity);

                return result > 0;
            }
        }

        private const string InsertQuery = @"
insert into {0}(id, wallet_id, asset_id, assetpair_id, amount, price, type, state, timestamp, fee_size, context)
    values (@Id, @WalletId, @AssetId, @AssetPairId, @Amount, @Price, @Type, @State, @Timestamp, @FeeSize, @Context)
ON CONFLICT (id) DO NOTHING;
";
    }
}
