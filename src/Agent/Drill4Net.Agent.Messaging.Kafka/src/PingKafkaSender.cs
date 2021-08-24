using System;
using System.Collections.Specialized;
using Confluent.Kafka;

namespace Drill4Net.Agent.Messaging.Kafka
{
    public class PingKafkaSender : AbstractKafkaSender, IPingSender
    {
        private IProducer<string, StringDictionary> _producer;
        private bool _isSending;

        /*************************************************************************/

        public PingKafkaSender(IMessageSenderRepository rep): base(rep)
        {
        }

        /*************************************************************************/

        public void SendPing(StringDictionary state, string topic = null)
        {
            if (_isSending)
                return;
            _isSending = true;
            if (topic == null)
                topic = MessagingConstants.TOPIC_PING;

            try
            {
                var mess = new Message<string, StringDictionary>
                {
                    Key = _rep.TargetSession.ToString(),
                    Value = state,
                    Headers = _headers
                };
                _producer.Produce(topic, mess, HandleStringStringDictData);
            }
            catch(Exception ex)
            {
                throw;
            }
            finally
            {
                _isSending = false;
            }
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
