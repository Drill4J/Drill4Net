using System;
using System.Collections.Generic;
using Confluent.Kafka;

namespace Drill4Net.Agent.Messaging.Kafka
{
    public class PingKafkaSender : AbstractKafkaSender, IPingSender
    {
        private readonly IProducer<string, Dictionary<string, string>> _producer;

        /*************************************************************************/

        public PingKafkaSender(IMessageSenderRepository rep): base(rep)
        {
        }

        /*************************************************************************/

        public void SendPing(Dictionary<string, string> state, string topic = MessagingConstants.TOPIC_PING)
        {
            throw new NotImplementedException();
        }

        protected override void AddSpecificHeaders()
        {
            throw new NotImplementedException();
        }

        protected override void CreateProducers()
        {
            throw new NotImplementedException();
        }

        private void HandleStringStringData(DeliveryReport<string, string> report)
        {
            Handle(report.Error);
        }

        /// <summary>
        /// Concretes the disposing.
        /// </summary>
        /// <returns></returns>
        protected override void ConcreteDisposing()
        {
            _producer?.Dispose();
        }

        protected override string GetMessageType()
        {
            throw new NotImplementedException();
        }
    }
}
