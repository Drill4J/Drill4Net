namespace Drill4Net.Agent.Kafka.Transport
{
    public delegate void ErrorOccuredDelegate(bool isFatal, bool isLocal, string message);

    /*************************************************************************************************/

    public interface IProbeReceiver
    {
        event ErrorOccuredDelegate ErrorOccured;

        void Start();
        void Stop();
    }
}