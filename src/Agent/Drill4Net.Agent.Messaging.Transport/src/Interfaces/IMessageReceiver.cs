namespace Drill4Net.Agent.Messaging.Transport
{
    public delegate void ErrorOccuredDelegate(IMessageReceiver source, bool isFatal, bool isLocal, string message);

    /*************************************************************************************************/

    public interface IMessageReceiver
    {
        event ErrorOccuredDelegate ErrorOccured;

        bool IsStarted { get; }
        void Start();
        void Stop();
    }
}