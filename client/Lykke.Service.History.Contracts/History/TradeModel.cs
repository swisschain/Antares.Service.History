using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.History.Contracts.Enums;

namespace Lykke.Service.History.Contracts.History
{
    public class TradeModel : BaseHistoryModel
    {
        public override HistoryType Type => HistoryType.Trade;

        public string AssetPairId { get; set; }

        public decimal BaseVolume { get; set; }

        public string BaseAssetId { get; set; }

        public string QuotingAssetId { get; set; }

        public decimal QuotingVolume { get; set; }

        public decimal Price { get; set; }

        public decimal? FeeSize { get; set; }

        public string FeeAssetId { get; set; }

        public int Index { get; set; }

        public TradeRole Role { get; set; }

        public Guid OrderId { get; set; }
    }
}
