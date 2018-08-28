using Lykke.Service.History.Core.Domain.Enums;

namespace Lykke.Service.History.Core.Domain.History
{
    public class Transfer : BaseHistoryRecord
    {
        public decimal Volume { get; set; }

        public string AssetId { get; set; }

        public decimal? FeeSize { get; set; }

        public override HistoryType Type => HistoryType.Transfer;
    }
}
