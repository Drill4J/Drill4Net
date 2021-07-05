using System;
using Newtonsoft.Json;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Transport
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184

    /// <summary>
    /// Data Sender for the profiling Agent
    /// </summary>
    /// <seealso cref="Drill4Net.Agent.Abstract.AbstractSender" />
    public class AgentSender : AbstractSender
    {
        private readonly Connector _connector;
        private readonly JsonSerializerSettings _deserOpts;

        /************************************************************************/

        public AgentSender(Connector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _deserOpts = new JsonSerializerSettings
            {

            };
        }

        /************************************************************************/

        protected override string Serialize(object data)
        {
            //return JsonSerializer.Serialize(data);
            return JsonConvert.SerializeObject(data, _deserOpts);
        }

        protected override void SendConcrete(string messageType, string topic, string message)
        {
            _connector.SendMessage(messageType, topic, message);
        }

        protected override void SendToPluginConcrete(string topic, string message)
        {
            _connector.SendPluginMessage(topic, message);
        }

        #region Test
        public override void SendOutgoingTest(OutgoingMessage data)
        {

        }
        
        public override void SendOutgoingTest(string topic, OutgoingMessage data)
        {

        }

        public override void SendIncomingTest(string topic, IncomingMessage message)
        {
            
        }
        #endregion
    }
}