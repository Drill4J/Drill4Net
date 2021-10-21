using System;
using Newtonsoft.Json;
using Drill4Net.Common;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Transport
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184

    /// <summary>
    /// Data Sender for the profiling Agent
    /// </summary>
    /// <seealso cref="Drill4Net.Agent.Abstract.AbstractCoveragerSender" />
    public class AgentSender : AbstractCoveragerSender
    {
        private readonly Connector _connector;
        private readonly JsonSerializerSettings _deserOpts;

        /************************************************************************/

        public AgentSender(Connector connector): base(CoreConstants.SUBSYSTEM_AGENT)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _deserOpts = new JsonSerializerSettings
            {
            };
        }

        /************************************************************************/

        protected override string Serialize(object data)
        {
            return JsonConvert.SerializeObject(data, _deserOpts);
        }

        protected override void SendConcrete(string messageType, string route, string message)
        {
            _connector.SendMessage(messageType, route, message);
        }

        protected override void SendToPluginConcrete(string pluginId, string message)
        {
            _connector.SendPluginMessage(pluginId, message);
        }

        #region Debug
        public override void DebugSendOutgoingTest(OutgoingMessage data)
        {

        }
        
        public override void DebugSendOutgoingTest(string topic, OutgoingMessage data)
        {

        }

        public override void DebugSendIncomingTest(string topic, IncomingMessage message)
        {
            
        }
        #endregion
    }
}