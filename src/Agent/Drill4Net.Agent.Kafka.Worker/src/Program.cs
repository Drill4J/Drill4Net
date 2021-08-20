using Drill4Net.Common;
using Drill4Net.Agent.Kafka.Transport;

namespace Drill4Net.Agent.Kafka.Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            AbstractRepository<CommunicatorOptions> rep = new KafkaConsumerRepository();
            IKafkaWorkerReceiver consumer = new KafkaWorkerReceiver(rep);
            var agent = new CoverageWorker(consumer);

            agent.Start();
        }
    }
}
