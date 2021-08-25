using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.Messaging.Transport;

namespace Drill4Net.Agent.Worker
{
    public delegate void ProbeReceivedHandler(Probe probe);

    /********************************************************************************************/

    public interface IProbeReceiver : IMessageReceiver
    {
        event ProbeReceivedHandler ProbeReceived;
    }
}