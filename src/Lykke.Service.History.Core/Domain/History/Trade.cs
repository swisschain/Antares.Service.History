using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.History.Core.Domain.History
{
    public class Trade : BaseHistoryRecord
    {
        public string AssetPairId { get; set; }

        public string AssetId { get; set; }

        public decimal Price { get; set; }

        public decimal FeeSize { get; set; }

        public string FeeAssetId { get; set; }

        public int Index { get; set; }

        public TradeRole Role { get; set; }

        public string OppositeAssetId { get; set; }

        public decimal OppositeVolume { get; set; }

        public Guid OrderId { get; set; }
    }
}
