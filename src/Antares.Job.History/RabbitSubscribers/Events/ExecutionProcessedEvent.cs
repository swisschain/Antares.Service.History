using System.Collections.Generic;
using ProtoBuf;
using OrderModel = Antares.Job.History.RabbitSubscribers.Models.OrderModel;

namespace Antares.Job.History.RabbitSubscribers.Events
{
    /// <summary>
    /// Exectuion processed event
    /// </summary>
    [ProtoContract]
    public class ExecutionProcessedEvent
    {
        [ProtoMember(1, IsRequired = true)]
        public IReadOnlyList<OrderModel> Orders { get; set; }

        [ProtoMember(2, IsRequired = true)]
        public long SequenceNumber { get; set; }
    }
}
