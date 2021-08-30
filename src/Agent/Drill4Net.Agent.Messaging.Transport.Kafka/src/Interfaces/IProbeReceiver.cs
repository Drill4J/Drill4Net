namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    public delegate void ProbeReceivedHandler(Probe probe);

    /*************************************************************/

    public interface IProbeReceiver : IMessageReceiver
    {
        event ProbeReceivedHandler ProbeReceived;
    }
}