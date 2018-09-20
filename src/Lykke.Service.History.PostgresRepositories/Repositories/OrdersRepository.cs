using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.History.PostgresRepositories.Entities;
using Lykke.Service.History.PostgresRepositories.Mappings;
using Microsoft.EntityFrameworkCore;
using PostgreSQLCopyHelper;

namespace Lykke.Service.History.PostgresRepositories.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private static readonly PostgreSQLCopyHelper<OrderEntity> BulkMapping;

        private readonly string _bulkUpsertQuery = $@"
insert into {Constants.OrdersTableName}
select * from {Constants.TempOrdersTableName}
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
            where {Constants.OrdersTableName}.sequence_number < excluded.sequence_number";

        private readonly ConnectionFactory _connectionFactory;

        private readonly string _createTempTableQuery = $@"
create temp table if not exists {Constants.TempOrdersTableName} 
(like {Constants.OrdersTableName})
on commit drop";

        private readonly string _insertOrUpdateQuery = $@"
insert into {Constants.OrdersTableName}(id, matching_id, wallet_id, assetpair_id, type, side, status, volume, price, 
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
            where {Constants.OrdersTableName}.sequence_number < @SequenceNumber";

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
                var result = await connection.ExecuteAsync(_insertOrUpdateQuery, order);

                return result > 0;
            }
        }

        public async Task UpsertBulkAsync(IEnumerable<Order> records)
        {
            using (var connection = await _connectionFactory.CreateNpgsqlConnection())
            {
                using (var tx = connection.BeginTransaction())
                {
                    await connection.QueryAsync(_createTempTableQuery);

                    var mapped = records.Select(Mapper.Map<OrderEntity>);

                    // if there are more than one order with the same id, we need to send only latest
                    // otherwise PostgreSQL will throw exception 21000: command cannot affect row a second time
                    var uniqueOrders = mapped.GroupBy(x => x.Id)
                        .Select(x => x.OrderByDescending(o => o.SequenceNumber).First());

                    BulkMapping.SaveAll(connection, uniqueOrders);

                    await connection.QueryAsync(_bulkUpsertQuery);

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

        public async Task<IEnumerable<Order>> GetOrders(Guid walletId, OrderType[] types, OrderStatus[] statuses, string assetPairId, int offset, int limit)
        {
            using (var context = _connectionFactory.CreateDataContext())
            {
                var query = context.Orders
                    .Where(x => x.WalletId == walletId && statuses.Contains(x.Status) && types.Contains(x.Type))
                    .Where(x => string.IsNullOrWhiteSpace(assetPairId) || x.AssetPairId == assetPairId)
                    .OrderByDescending(x => x.CreateDt)
                    .Skip(offset)
                    .Take(limit);

                return (await query.ToListAsync()).Select(Mapper.Map<Order>);
            }
        }
    }
}
