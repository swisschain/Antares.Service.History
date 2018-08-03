using System;
using MessagePack;

namespace Lykke.Service.History.Contracts.Cqrs.Commands
{
    /// <summary>
    /// Save cashin to history command
    /// </summary>
    [MessagePackObject(true)]
    public class SaveCashinCommand
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }

        public decimal Volume { get; set; }

        public string AssetId { get; set; }

        public DateTime Timestamp { get; set; }

        public decimal? FeeSize { get; set; }
    }
}
