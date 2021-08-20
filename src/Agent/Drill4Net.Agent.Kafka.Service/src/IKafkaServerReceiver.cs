using Drill4Net.Agent.Kafka.Transport;

namespace Drill4Net.Agent.Kafka.Service
{
    public interface IKafkaServerReceiver : IProbeReceiver
    {
        event TargetReceivedInfoHandler TargetInfoReceived;
    }
}