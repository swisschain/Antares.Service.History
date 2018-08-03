using Lykke.Service.History.Core.Domain.Enums;

namespace Lykke.Service.History.Core.Domain.History
{
    public class Cashin : BaseHistoryRecord
    {
        public string AssetId { get; set; }

        public string BlockchainHash { get; set; }

        public HistoryState State { get; set; }

        public decimal FeeSize { get; set; }
    }
}
