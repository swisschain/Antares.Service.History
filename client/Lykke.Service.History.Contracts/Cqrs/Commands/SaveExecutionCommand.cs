using System.Collections.Generic;
using Lykke.Service.History.Contracts.Cqrs.Models;
using MessagePack;

namespace Lykke.Service.History.Contracts.Cqrs.Commands
{
    /// <summary>
    /// Save execution result to history command
    /// </summary>
    [MessagePackObject(true)]
    public class SaveExecutionCommand
    {
        public List<OrderModel> Orders { get; set; }

        public long SequenceNumber { get; set; }
    }
}
