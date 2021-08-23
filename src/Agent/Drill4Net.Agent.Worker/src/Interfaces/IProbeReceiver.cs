using Drill4Net.Agent.Messaging.Transport;

namespace Drill4Net.Agent.Worker
{
    public interface IProbeReceiver : IMessageReceiver
    {
        event ProbeReceivedHandler ProbeReceived;
    }
}