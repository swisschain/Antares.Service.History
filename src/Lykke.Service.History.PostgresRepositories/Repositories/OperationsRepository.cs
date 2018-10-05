using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Operations;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.History.PostgresRepositories.Entities;
using Lykke.Service.History.PostgresRepositories.JsonbQuery;
using Lykke.Service.History.PostgresRepositories.Mappings;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PostgreSQLCopyHelper;

namespace Lykke.Service.History.PostgresRepositories.Repositories
{
    public class OperationsRepository : IOperationsRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        private readonly string _insertQuery = $@"
insert into {Constants.OperationsTableName}(id, type, create_dt)
    values (@Id, @Type, @CreateDt)
ON CONFLICT (id) DO NOTHING;
";

        public OperationsRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        public async Task<Operation> Get(Guid id)
        {
            using (var context = _connectionFactory.CreateDataContext())
            {
                return Mapper.Map<Operation>(await context.Operations.FirstOrDefaultAsync(x => x.Id == id));
            }
        }

        public async Task<bool> TryInsertAsync(Operation operation)
        {
            using (var connection = await _connectionFactory.CreateNpgsqlConnection())
            {
                var result = await connection.ExecuteAsync(_insertQuery, Mapper.Map<OperationEntity>(operation));

                return result > 0;
            }
        }
    }
}
