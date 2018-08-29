using System;
using Lykke.Service.History.Contracts.Enums;

namespace Lykke.Service.History.Contracts.History
{
    public abstract class BaseHistoryModel
    {
        public abstract HistoryType Type { get; }

        public Guid Id { get; set; }

        public Guid WalletId { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
