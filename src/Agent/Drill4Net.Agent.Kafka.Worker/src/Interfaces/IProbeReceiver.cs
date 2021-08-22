using Drill4Net.Agent.Kafka.Transport;

namespace Drill4Net.Agent.Kafka.Worker
{
    public interface IProbeReceiver : IMessageReceiver
    {
        event ProbeReceivedHandler ProbeReceived;
    }
}