using System;
using Lykke.Service.History.Contracts.Cqrs.Commands.Models;

namespace Lykke.Service.History.Contracts.Cqrs.Models
{
    /// <summary>
    /// User trade item
    /// </summary>
    public class TradeModel
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }

        public string AssetPairId { get; set; }

        public string AssetId { get; set; }

        public decimal Volume { get; set; }

        public decimal Price { get; set; }

        public DateTime Timestamp { get; set; }

        public string OppositeAssetId { get; set; }

        public decimal OppositeVolume { get; set; }

        public int Index { get; set; }

        public TradeRole Role { get; set; }

        public decimal? FeeSize { get; set; }

        public string FeeAssetId { get; set; }
    }
}
