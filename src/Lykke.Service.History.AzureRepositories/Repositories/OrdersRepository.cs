using System;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Lykke.Service.History.Core.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Service.History.PostgresRepositories.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly ConnectionFactory _connectionFactory;

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

        public async Task<Order> Get(Guid id)
        {
            using (var context = _connectionFactory.CreateDataContext())
            {
                return Mapper.Map<Order>(await context.Orders.FirstOrDefaultAsync(x => x.Id == id));
            }
        }

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
            where {0}.sequence_number > @SequenceNumber
";
    }
}
