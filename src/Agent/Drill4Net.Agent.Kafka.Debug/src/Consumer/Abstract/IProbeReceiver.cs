namespace Drill4Net.Agent.Kafka.Debug
{
    public interface IProbeReceiver
    {
        event ReceivedTargetInfoHandler TargetInfoReceived;
        event ReceivedMessageHandler MessageReceived;
        event ErrorOccuredHandler ErrorOccured;

        void Start();
        void Stop();
    }
}