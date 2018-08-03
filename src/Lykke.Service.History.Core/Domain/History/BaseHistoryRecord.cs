using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.History.Core.Domain.History
{
    public class BaseHistoryRecord
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }

        public DateTime Timestamp { get; set; }

        public decimal Volume { get; set; }
    }
}
