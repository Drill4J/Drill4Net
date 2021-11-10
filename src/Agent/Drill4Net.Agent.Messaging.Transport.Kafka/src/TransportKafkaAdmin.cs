using System;
using System.Linq;
using System.Collections.Generic;
using Confluent.Kafka;

namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    public class TransportKafkaAdmin : AbstractTransportAdmin
    {
        private readonly List<string> _servers;

        /*********************************************************************************/

        public TransportKafkaAdmin(List<string> servers)
        {
            _servers = servers ?? throw new ArgumentNullException(nameof(servers));
        }

        /*********************************************************************************/

        public override List<string> GetAllTopics(IEnumerable<string> brokerList = null)
        {
            if (brokerList?.Any() != true)
                brokerList = _servers;
            //
            using var adminClient = new AdminClientBuilder(GetClientConfig(brokerList)).Build();
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            return metadata.Topics.ConvertAll(a => a.Topic);
        }

        public override void DeleteTopics(IEnumerable<string> topicNameList, IEnumerable<string> brokerList = null)
        {
            if (topicNameList?.Any() != true)
                return;
            if (brokerList?.Any() != true)
                brokerList = _servers;
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
