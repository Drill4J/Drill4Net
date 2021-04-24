using System;
using Websocket.Client;
using Websocket.Client.Models;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transport
{
    /// <summary>
    /// Receiver of data from admin side
    /// </summary>
    public class AgentReceiver : AbstractReceiver
    {
        private readonly WebsocketClient _receiver;

        /************************************************************************/

        public AgentReceiver(WebsocketClient receiver)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            _receiver.ReconnectionHappened.Subscribe(info => ReconnectHandler(info));
            _receiver.MessageReceived.Subscribe(msg => ReceivedHandler(msg));
            _receiver.Start();
        }

        /************************************************************************/

        private void ReceivedHandler(ResponseMessage message)
        {

        }

        private void ReconnectHandler(ReconnectionInfo info)
        {

        }
    }
}