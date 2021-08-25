namespace Drill4Net.Agent.Messaging.Transport
{
    public delegate void ErrorOccuredDelegate(bool isFatal, bool isLocal, string message);

    /*************************************************************************************************/

    public interface IMessageReceiver
    {
        event ErrorOccuredDelegate ErrorOccured;

        void Start();
        void Stop();
    }
}