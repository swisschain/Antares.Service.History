using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Service.History.Contracts.Enums;

namespace Lykke.Service.History.Contracts.History
{
    public class TransferModel : BaseHistoryModel
    {
        public override HistoryType Type => HistoryType.Transfer;

        public decimal Volume { get; set; }

        public string AssetId { get; set; }

        public decimal? FeeSize { get; set; }
    }
}
