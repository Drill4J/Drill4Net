using System.Collections.Generic;
using System;
using System.Linq;
using Confluent.Kafka;

namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    public class TransportKafkaAdmin : TransportAdmin
    {
        public override void DeleteTopics(IEnumerable<string> brokerList, IEnumerable<string> topicNameList)
        {
            if (brokerList?.Any() != true)
                throw new ArgumentNullException(nameof(brokerList));
            if (topicNameList?.Any() != true)
                return;
            //
            var bld = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = string.Join(",", brokerList) });
            using var client = bld.Build();
            client.DeleteTopicsAsync(topicNameList, null);
        }
    }
}
