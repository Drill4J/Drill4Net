namespace Drill4Net.Agent.Kafka.Debug
{
    public interface IProbeReceiver
    {
        event ErrorOccuredHandler ErrorOccured;
        event ReceivedMessageHandler MessageReceived;

        void Start();
    }
}