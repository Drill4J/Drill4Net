using System;
using System.Net.WebSockets;
using Websocket.Client;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transport
{
    public class Communicator : AbstractCommunicator
    {
        public Communicator(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));
            //
            //https://github.com/Marfusios/websocket-client
            var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
            {
                Options =
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(15),
                    //Credentials = ...
                    //Proxy = ...
                    //ClientCertificates = ...
                }
            });
            var client = new WebsocketClient(new Uri(url), factory)
            {
                ReconnectTimeout = TimeSpan.FromSeconds(15),
                ErrorReconnectTimeout = TimeSpan.FromSeconds(15),
            };
            Receiver = new AgentReceiver(client);
            Sender = new AgentSender(client);
        }
    }
}
