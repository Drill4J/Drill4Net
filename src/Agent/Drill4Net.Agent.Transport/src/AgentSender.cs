﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Websocket.Client;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;

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

        protected override string Serialize(object data)
        {
            return JsonSerializer.Serialize(data);
        }

        protected override void SendConcrete(string data)
        {
            _sender.Send(data);
        }

        public override void SendTest(AbstractMessage data)
        {
            SendFakeMessage(data);
        }
        
        public override void SendTest(IncomingMessage data)
        {
            SendFakeMessage(data);
        }

        private void SendFakeMessage(object data)
        {
            var serData = Serialize(data);
            var mess = ResponseMessage.TextMessage(serData);
            _sender.StreamFakeMessage(mess);
        }
    }
}