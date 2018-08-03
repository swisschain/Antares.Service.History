using Lykke.Service.History.Core.Domain.Enums;

namespace Lykke.Service.History.Core.Domain.History
{
    public class Transfer : BaseHistoryRecord
    {
        public string AssetId { get; set; }

        public decimal FeeSize { get; set; }
    }
}
