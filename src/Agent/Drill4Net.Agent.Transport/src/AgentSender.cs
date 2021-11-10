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
            _deserOpts = new JsonSerializerSettings();
        }

        /************************************************************************/

        protected override void SendMessageConcrete(string messageType, string route, string message)
        {
            _connector.SendMessage(messageType, route, message);
        }

        protected override void SendMessageToPluginConcrete(string pluginId, string message)
        {
            _connector.SendPluginMessage(pluginId, message);
        }

        protected override void SendActionToPluginConcrete(string pluginId, string message)
        {
            _connector.SendPluginAction(pluginId, message);
        }

        /// <summary>
        /// Start the session on Drill Admin side
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="sessionId"></param>
        /// <param name="isRealtime"></param>
        /// <param name="isGlobal"></param>
        protected override void StartSessionConcrete(string pluginId, string sessionId, bool isRealtime, bool isGlobal)
        {
            _connector.StartSession(pluginId, sessionId, isRealtime, isGlobal);
        }

        /// <summary>
        /// Stop the session on Drill Admin side
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="sessionId"></param>
        protected override void StopSessionConcrete(string pluginId, string sessionId)
        {
            _connector.StopSession(pluginId, sessionId);
        }

        /// <summary>
        /// Register info about running tests.
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="tests2Run"></param>
        public override void RegisterTestsRunConcrete(string pluginId, string tests2Run)
        {
            _connector.AddTestsRun(pluginId, tests2Run);
        }
        
        protected override string Serialize(object data)
        {
            return JsonConvert.SerializeObject(data, _deserOpts);
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