using System.Collections.Generic;
using System;
using System.Linq;
using Confluent.Kafka;

namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    public class TransportKafkaAdmin : AbstractTransportAdmin
    {
        public override List<string> GetAllTopics(IEnumerable<string> brokerList)
        {
            using var adminClient = new AdminClientBuilder(GetClientConfig(brokerList)).Build();
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            var topicsMetadata = metadata.Topics;
            return metadata.Topics.ConvertAll(a => a.Topic);
        }

        public override void DeleteTopics(IEnumerable<string> brokerList, IEnumerable<string> topicNameList)
        {
            if (brokerList?.Any() != true)
                throw new ArgumentNullException(nameof(brokerList));
            if (topicNameList?.Any() != true)
                return;
            //
            using var adminClient = new AdminClientBuilder(GetClientConfig(brokerList)).Build();
            adminClient.DeleteTopicsAsync(topicNameList, null);
        }

        private AdminClientConfig GetClientConfig(IEnumerable<string> brokerList)
        {
            return new AdminClientConfig { BootstrapServers = string.Join(",", brokerList) };
        }
    }
}
