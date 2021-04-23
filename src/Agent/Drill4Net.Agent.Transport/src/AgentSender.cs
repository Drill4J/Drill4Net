using System;
using Websocket.Client;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transport
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184
    
    public class AgentSender : AbstractSender
    {
        private readonly WebsocketClient _sender;

        /************************************************************************/

        public AgentSender(WebsocketClient sender)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        /************************************************************************/

        public override void Send(string messageType, string message)
        {
            _sender.Send(messageType + " " + message);
        }
    }
}