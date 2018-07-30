using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.History.Core.Domain;

namespace Lykke.Service.History.PostgresRepositories.Entities
{
    public class HistoryEntity
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }

        public string AssetId { get; set; }

        public string AssetPairId { get; set; }

        public decimal Amount { get; set; }

        public decimal? Price { get; set; }

        public HistoryType Type { get; set; }

        public HistoryState State { get; set; }

        public DateTime Timestamp { get; set; }

        public decimal? FeeSize { get; set; }

        public string Context { get; set; }
    }
}
