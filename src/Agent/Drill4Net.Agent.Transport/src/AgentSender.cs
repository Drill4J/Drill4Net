using System;
using Websocket.Client;
using Newtonsoft.Json;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Messages;

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

        protected override string ConvertToPayload(string messageType, object data)
        {
            var payload = new ConnectorQueueItem()
            {
                Destination =  messageType,
                Message = JsonConvert.SerializeObject(data)
            };
            return JsonConvert.SerializeObject(payload);
        }

        protected override void SendConcrete(string payload)
        {
            _sender.Send(payload);
        }

        public override void SendTest(string messageType, object data)
        {
            var payload = ConvertToPayload(messageType, data);
            var mess = ResponseMessage.TextMessage(payload);
            _sender.StreamFakeMessage(mess);
        }
    }
}