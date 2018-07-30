using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace Lykke.Service.History.PostgresRepositories
{
    public class NpgsqlConnectionProvider
    {
        private readonly string _connectionString;

        public NpgsqlConnectionProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<NpgsqlConnection> GetOpenedConnection()
        {
            var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            return conn;
        }
    }
}
