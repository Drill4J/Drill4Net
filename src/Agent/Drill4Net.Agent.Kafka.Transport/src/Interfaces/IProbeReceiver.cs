namespace Drill4Net.Agent.Kafka.Transport
{
    public interface IProbeReceiver
    {
        event ErrorOccuredDelegate ErrorOccured;

        void Start();
        void Stop();
    }
}