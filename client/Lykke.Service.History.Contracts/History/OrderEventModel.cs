using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.History.Contracts.Enums;

namespace Lykke.Service.History.Contracts.History
{
    public class OrderEventModel : BaseHistoryModel
    {
        public override HistoryType Type => HistoryType.OrderEvent;

        public decimal Volume { get; set; }

        public string AssetPairId { get; set; }

        public Guid OrderId { get; set; }

        public decimal Price { get; set; }

        public OrderStatus Status { get; set; }
    }
}
