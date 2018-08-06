using Lykke.Service.History.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.History.Core.Domain.History
{
    public class OrderEvent : BaseHistoryRecord
    {
        public string AssetPairId { get; set; }

        public Guid OrderId { get; set; }

        public decimal Price { get; set; }

        public OrderStatus Status { get; set; }

        public override HistoryType Type => HistoryType.OrderEvent;
    }
}
