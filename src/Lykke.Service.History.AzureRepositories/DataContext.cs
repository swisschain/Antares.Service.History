using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.History.PostgresRepositories.Entities;
using Lykke.Service.History.PostgresRepositories.JsonbQuery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.Sql;

namespace Lykke.Service.History.PostgresRepositories
{
    public class DataContext : DbContext
    {
        private readonly string _connectionString;

        public DataContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        internal virtual DbSet<HistoryEntity> History { get; set; }

        internal virtual DbSet<OrderEntity> Orders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<ICompositeMethodCallTranslator, CustomSqlMethodCallTranslator>();

            optionsBuilder.UseNpgsql(_connectionString);
        }
    }
}
