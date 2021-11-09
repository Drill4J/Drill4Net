using System.Collections.Specialized;
using Confluent.Kafka;

namespace Drill4Net.Agent.Messaging.Kafka
{
    public class PingKafkaSender : AbstractKafkaSender, IPingSender
    {
        private IProducer<string, StringDictionary> _producer;

        /*************************************************************************/

        public PingKafkaSender(IMessagerRepository rep): base(rep)
        {
        }

        /*************************************************************************/

        public void SendPing(StringDictionary state, string topic = null)
        {
            if (topic == null)
                topic = MessagingConstants.TOPIC_PING;

            var mess = new Message<string, StringDictionary>
            {
                Key = _rep.TargetSession.ToString(),
                Value = state,
                Headers = _headers
            };
            _producer.Produce(topic, mess, HandleStringStringDictData);
        }

        protected override void CreateProducers()
        {
            _producer = new ProducerBuilder<string, StringDictionary>(_cfg)
                .SetValueSerializer(new StringDictionarySerializer())
                .Build();
        }

        private void HandleStringStringDictData(DeliveryReport<string, StringDictionary> report)
        {
            Handle(report.Error);
        }

        protected override string GetMessageType()
        {
            return MessagingConstants.MESSAGE_TYPE_PING;
        }

        /// <summary>
        /// Concretes the disposing.
        /// </summary>
        /// <returns></returns>
        protected override void ConcreteDisposing()
        {
            _producer?.Dispose();
        }
    }
}
