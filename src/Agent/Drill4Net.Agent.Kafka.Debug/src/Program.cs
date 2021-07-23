using System;

namespace Drill4Net.Agent.Kafka.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            var rep = new KafkaConsumerRepository();
            var agent = new KafkaConsumer(rep);
        }
    }
}
