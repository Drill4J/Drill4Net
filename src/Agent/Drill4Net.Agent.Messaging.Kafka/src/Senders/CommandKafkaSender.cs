using System;
using Confluent.Kafka;

namespace Drill4Net.Agent.Messaging.Kafka
{
    public class CommandKafkaSender : AbstractKafkaSender, ICommandSender
    {
        private IProducer<Null, Command> _producer;

        /**************************************************************************/

        public CommandKafkaSender(IMessageSenderRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public void SendCommand(int type, string data)
        {
            var topic = MessagingUtils.GetCommandTopic(_rep.TargetSession.ToString());
            var com = new Command { Type = type, Data = data };
            var mess = new Message<Null, Command> { Value = com, Headers = _headers };
            _producer.Produce(topic, mess, HandleProbeData);
            _producer.Flush(); //we must guarantee the delivery
        }

        private void HandleProbeData(DeliveryReport<Null, Command> report)
        {
            Handle(report.Error);
        }

        protected override void CreateProducers()
        {
            _producer = new ProducerBuilder<Null, Command>(_cfg)
                .SetValueSerializer(new CommandSerializer())
                .Build();
        }

        protected override string GetMessageType()
        {
            return MessagingConstants.MESSAGE_TYPE_COMMAND;
        }

        protected override void ConcreteDisposing()
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }
    }
}
