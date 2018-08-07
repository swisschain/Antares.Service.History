using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace Lykke.Service.History.Workflow.ExecutionProcessing
{
    public sealed class MessageAcceptor
    {
        private readonly IModel _model;
        private readonly ulong _deliveryTag;

        public MessageAcceptor(IModel model, ulong deliveryTag)
        {
            _model = model;
            _deliveryTag = deliveryTag;
        }

        public void Accept()
        {
            _model.BasicAck(_deliveryTag, false);
        }

        public void Reject()
        {
            _model.BasicReject(_deliveryTag, false);
        }
    }
}
