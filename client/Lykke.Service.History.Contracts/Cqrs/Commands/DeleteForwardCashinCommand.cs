using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace Lykke.Service.History.Contracts.Cqrs.Commands
{
    [ProtoContract]
    public class DeleteForwardCashinCommand
    {
        [ProtoMember(1)]
        public Guid OperationId { get; set; }

        [ProtoMember(2)]
        public Guid WalletId { get; set; }
    }
}
