using System;
using Lykke.Service.History.Core.Domain.Enums;

namespace Lykke.Service.History.Core.Domain.Operations
{
    public class Operation
    {
        public Guid Id { get; set; }

        public HistoryOperationType Type { get; set; }

        public DateTime CreateDt { get; set; }
    }
}
