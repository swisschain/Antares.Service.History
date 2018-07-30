using System;
using System.Collections.Generic;
using System.Text;
using Dapper.FluentMap.Dommel.Mapping;
using Dapper.FluentMap.Mapping;
using Lykke.Service.History.PostgresRepositories.Entities;
using PostgreSQLCopyHelper;

namespace Lykke.Service.History.PostgresRepositories.Mappings
{
    public class HistoryMap : DommelEntityMap<HistoryEntity>
    {
        public HistoryMap(string tableName, string shemaName)
        {
            ToTable(tableName, shemaName);
            
            Map(p => p.Id)
                .ToColumn("id");
            Map(p => p.WalletId)
                .ToColumn("wallet_id");
            Map(p => p.AssetId)
                .ToColumn("asset_id");
            Map(p => p.AssetPairId)
                .ToColumn("assetpair_id");
            Map(p => p.Amount)
                .ToColumn("amount");
            Map(p => p.Price)
                .ToColumn("price");
            Map(p => p.Type)
                .ToColumn("type");
            Map(p => p.State)
                .ToColumn("state");
            Map(p => p.Timestamp)
                .ToColumn("timestamp");
            Map(p => p.FeeSize)
                .ToColumn("fee_size");
            Map(p => p.Context)
                .ToColumn("context");
        }

        public static PostgreSQLCopyHelper<HistoryEntity> GetBulkMapping(string schema, string tableName)
        {
            return new PostgreSQLCopyHelper<HistoryEntity>(schema, tableName)
                .MapUUID("id", x => x.Id)
                .MapUUID("wallet_id", x => x.WalletId)
                .MapText("asset_id", x => x.AssetId)
                .MapText("assetpair_id", x => x.AssetPairId)
                .MapNumeric("amount", x => x.Amount)
                .MapNullable("price", x => x.Price, NpgsqlTypes.NpgsqlDbType.Numeric)
                .MapInteger("type", x => (int)x.Type)
                .MapInteger("state", x => (int)x.State)
                .MapTimeStamp("timestamp", x => x.Timestamp)
                .MapNullable("fee_size", x => x.FeeSize, NpgsqlTypes.NpgsqlDbType.Numeric)
                .MapJsonb("data", x => x.Context);
        }
    }
}
