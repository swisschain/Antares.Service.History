using System;
using MessagePack;

namespace Lykke.Service.History.Contracts.Cqrs.Commands
{
    [MessagePackObject(true)]
    public class SaveCashinCommand
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
