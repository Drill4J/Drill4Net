using System;
using Websocket.Client;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transport
{
    //https://github.com/Marfusios/websocket-client

    public class Communicator : AbstractCommunicator
    {
        public Communicator(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));
            //
            var client = new WebsocketClient(new Uri(url))
            {
                ReconnectTimeout = TimeSpan.FromSeconds(15),
                ErrorReconnectTimeout = TimeSpan.FromSeconds(15),
            };
            Receiver = new AgentReceiver(client);
            Sender = new AgentSender(client);
        }
    }
}
