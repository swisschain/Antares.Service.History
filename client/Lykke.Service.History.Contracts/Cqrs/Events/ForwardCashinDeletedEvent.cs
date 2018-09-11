using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace Lykke.Service.History.Contracts.Cqrs.Events
{
    [ProtoContract]
    public class ForwardCashinDeletedEvent
    {
        [ProtoMember(1)]
        public Guid OperationId { get; set; }

        [ProtoMember(2)]
        public Guid WalletId { get; set; }
    }
}
