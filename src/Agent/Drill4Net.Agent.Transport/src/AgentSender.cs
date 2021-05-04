using System;
using System.Text.Json;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Transport
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184
    
    public class AgentSender : AbstractSender
    {
        private readonly Connector _connector;

        /************************************************************************/

        public AgentSender(Connector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        /************************************************************************/

        protected override string Serialize(object data)
        {
            return JsonSerializer.Serialize(data);
        }

        protected override void SendConcrete(string messageType, string topic, string message)
        {
            _connector.SendMessage(messageType, topic, message);
        }

        protected override void SendToPluginConcrete(string topic, string message)
        {
            _connector.SendPluginMessage(topic, message);
        }

        public override void SendTest(AbstractMessage data)
        {
            //SendFakeMessage(data);
        }
        
        public override void SendTest(IncomingMessage data)
        {
            //SendFakeMessage(data);
        }

        private void SendFakeMessage(object data)
        {
            //var serData = Serialize(data);
            //var mess = ResponseMessage.TextMessage(serData);
            //_sender.StreamFakeMessage(mess);
        }


    }
}