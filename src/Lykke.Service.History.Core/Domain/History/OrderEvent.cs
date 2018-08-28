using System;
using Lykke.Service.History.Core.Domain.Enums;

namespace Lykke.Service.History.Core.Domain.History
{
    public class OrderEvent : BaseHistoryRecord
    {
        public decimal Volume { get; set; }

        public string AssetPairId { get; set; }

        public Guid OrderId { get; set; }

        public decimal Price { get; set; }

        public OrderStatus Status { get; set; }

        public override HistoryType Type => HistoryType.OrderEvent;
    }
}
