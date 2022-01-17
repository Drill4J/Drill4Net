using System;
using System.Collections.Generic;
using Confluent.Kafka;
using Drill4Net.BanderLog;

namespace Drill4Net.Agent.Messaging.Kafka
{
    public class CommandKafkaSender : AbstractKafkaSender, ICommandSender
    {
        private IProducer<Null, Command> _producer;
        private readonly Logger _logger;

        /**************************************************************************/

        public CommandKafkaSender(IMessagerRepository rep) : base(rep)
        {
            _logger = new TypedLogger<CommandKafkaSender>(rep.Subsystem);
            _logger.Debug($"Command sender servers: {string.Join(",", _rep.MessagerOptions.Servers)}");
        }

        /**************************************************************************/

        public void SendCommand(int type, string data, IEnumerable<string> topics)
        {
            var com = new Command { Type = type, Data = data };
            var mess = new Message<Null, Command> { Value = com, Headers = _headers };
            foreach (var topic in topics)
            {
                _logger.Debug($"Sending command [{type}] to topic [{topic}]");
                _producer.Produce(topic, mess, HandleProbeData);
            }
            Flush(); //we must guarantee the delivery
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
            Flush();
            _producer?.Dispose();
        }

        public void Flush()
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
        }
    }
}
