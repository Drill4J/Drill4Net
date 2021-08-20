namespace Drill4Net.Agent.Kafka.Service
{
    public interface IProbeReceiver
    {
        event TargetReceivedInfoHandler TargetInfoReceived;
        event ProbeReceivedHandler ProbeReceived;
        event ErrorOccuredHandler ErrorOccured;

        void Start();
        void Stop();
    }
}