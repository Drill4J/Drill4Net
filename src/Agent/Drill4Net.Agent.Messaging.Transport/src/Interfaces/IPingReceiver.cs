using System.Collections.Specialized;

namespace Drill4Net.Agent.Messaging.Transport
{
    public delegate void PingReceivedHandler(string targetSession, StringDictionary data);

    /**************************************************************************************/

    public interface IPingReceiver : IMessageReceiver
    {
        event PingReceivedHandler PingReceived;
    }
}
