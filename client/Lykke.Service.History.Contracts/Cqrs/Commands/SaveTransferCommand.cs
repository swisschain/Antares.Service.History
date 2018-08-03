using System;
using MessagePack;

namespace Lykke.Service.History.Contracts.Cqrs.Commands
{
    /// <summary>
    /// Save transfer to history command
    /// </summary>
    [MessagePackObject(true)]
    public class SaveTransferCommand
    {
        public Guid Id { get; set; }

        public Guid FromWalletId { get; set; }

        public Guid ToWalletId { get; set; }

        public decimal Volume { get; set; }

        public string AssetId { get; set; }

        public DateTime Timestamp { get; set; }

        public Guid FeeWalletId { get; set; }

        public decimal? FeeSize { get; set; }
    }
}
