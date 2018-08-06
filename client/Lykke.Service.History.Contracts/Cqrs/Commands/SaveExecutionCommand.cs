using System.Collections.Generic;
using Lykke.Service.History.Contracts.Cqrs.Models;
using ProtoBuf;

namespace Lykke.Service.History.Contracts.Cqrs.Commands
{
    /// <summary>
    /// Save execution result to history command
    /// </summary>
    [ProtoContract]
    public class SaveExecutionCommand
    {
        [ProtoMember(1, IsRequired = true)]
        public List<OrderModel> Orders { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public long SequenceNumber { get; set; }
    }
}
