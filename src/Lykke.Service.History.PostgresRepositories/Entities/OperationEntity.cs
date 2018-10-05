using System;
using System.ComponentModel.DataAnnotations.Schema;
using Lykke.Service.History.Core.Domain.Enums;

namespace Lykke.Service.History.PostgresRepositories.Entities
{
    [Table(Constants.OperationsTableName)]
    internal class OperationEntity
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("type")]
        public HistoryOperationType Type { get; set; }

        [Column("create_dt")]
        public DateTime CreateDt { get; set; }
    }
}
