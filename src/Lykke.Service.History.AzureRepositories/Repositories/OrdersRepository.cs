using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.History.PostgresRepositories.Entities;
using Lykke.Service.History.PostgresRepositories.Mappings;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PostgreSQLCopyHelper;


namespace Lykke.Service.History.PostgresRepositories.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private static readonly PostgreSQLCopyHelper<OrderEntity> BulkMapping;

        private readonly ConnectionFactory _connectionFactory;

        static OrdersRepository()
        {
            BulkMapping = OrderEntityBulkMapping.Generate();
        }

        public OrdersRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> InsertOrUpdateAsync(Order order)
        {
            using (var connection = await _connectionFactory.CreateNpgsqlConnection())
            {
                var result = await connection.ExecuteAsync(string.Format(InsertOrUpdateQuery, Constants.OrdersTableName), order);

                return result > 0;
            }
        }

        public async Task UpsertBulkAsync(IEnumerable<Order> records)
        {
            using (var connection = await _connectionFactory.CreateNpgsqlConnection())
            {
                using (var tx = connection.BeginTransaction())
                {
                    await connection.QueryAsync(string.Format(CreateTempTableQuery, Constants.TempOrdersTableName,
                        Constants.OrdersTableName));

                    BulkMapping.SaveAll(connection, records.Select(Mapper.Map<OrderEntity>));

                    await connection.QueryAsync(string.Format(BulkUpsertQuery, Constants.TempOrdersTableName,
                        Constants.OrdersTableName));

                    await tx.CommitAsync();
                }
            }
        }

        public async Task<Order> Get(Guid id)
        {
            using (var context = _connectionFactory.CreateDataContext())
            {
                return Mapper.Map<Order>(await context.Orders.FirstOrDefaultAsync(x => x.Id == id));
            }
        }

        private const string CreateTempTableQuery = @"
create temp table if not exists {0} 
(like {1})
on commit drop";

        private const string BulkUpsertQuery = @"
insert into {1}
select * from {0}
ON CONFLICT (id) DO UPDATE
    set type = excluded.type,
        status = excluded.status,
        volume = excluded.volume,
        price = excluded.price,
        register_dt = excluded.register_dt,
        status_dt = excluded.status_dt,
        match_dt = excluded.match_dt,
        remaining_volume = excluded.remaining_volume,
        reject_reason = excluded.reject_reason,
        lower_limit_price = excluded.lower_limit_price,
        lower_price = excluded.lower_price,
        upper_limit_price = excluded.upper_limit_price,
        upper_price = excluded.upper_price,
        sequence_number = excluded.sequence_number
            where {1}.sequence_number < excluded.sequence_number
";

        private const string InsertOrUpdateQuery = @"
insert into {0}(id, matching_id, wallet_id, assetpair_id, type, side, status, volume, price, 
                create_dt, register_dt, status_dt, match_dt, remaining_volume, reject_reason, 
                lower_limit_price, lower_price, upper_limit_price, upper_price, straight, sequence_number)
    values (@Id, @MatchingId, @WalletId, @AssetPairId, @Type, @Side, @Status, @Volume, @Price,
            @CreateDt, @RegisterDt, @StatusDt, @MatchDt, @RemainingVolume, @RejectReason, 
            @LowerLimitPrice, @LowerPrice, @UpperLimitPrice, @UpperPrice, @Straight, @SequenceNumber)
ON CONFLICT (id) DO UPDATE
    set type = @Type,
        status = @Status,
        volume = @Volume,
        price = @Price,
        register_dt = @RegisterDt,
        status_dt = @StatusDt,
        match_dt = @MatchDt,
        remaining_volume = @RemainingVolume,
        reject_reason = @RejectReason,
        lower_limit_price = @LowerLimitPrice,
        lower_price = @LowerPrice,
        upper_limit_price = @UpperLimitPrice,
        upper_price = @UpperPrice,
        sequence_number = @SequenceNumber
            where {0}.sequence_number < @SequenceNumber
";
    }
}
