using System.Collections.Generic;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    public abstract class AbstractSender : ISender
    {
        #region Send messages
        #region Init
        /// <summary>
        /// "Send Scope Initialized" message ("SCOPE_INITIALIZED")
        /// </summary>
        public virtual void SendScopeInitialized(InitActiveScope scope, long ts)
        {
            var load = scope.Payload;
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, 
                new ScopeInitialized(load.Id, load.Name, load.PrevId, ts));
        }
        
        /// <summary>
        /// "Agent is starting init process" message ("INIT")
        /// </summary>
        public virtual void SendInitMessage(int classesCount)
        {
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, new InitInfo(classesCount));
        }

        /// <summary>
        /// "INIT_DATA_PART"
        /// </summary>
        /// <param name="entities"></param>
        public virtual void SendClassesDataMessage(List<AstEntity> entities)
        {
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, new InitDataPart { astEntities = entities });
        }

        /// <summary>
        /// "Agent is initialized" message ("INITIALIZED")
        /// </summary>
        public virtual void SendInitializedMessage()
        {        
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, new Initialized { msg = "Initialized" }); //can be any string
        }
        #endregion

        public virtual void SendSessionStartedMessage(string sessionUid, string testType, bool isRealTime, long ts)
        {
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionStarted
                {
                    sessionId = sessionUid,
                    isRealtime = isRealTime,
                    testType = testType,
                    ts = ts,
                });
        }
        
        public virtual void SendSessionFinishedMessage(string sessionUid, long ts)
        {
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionFinished { sessionId = sessionUid, ts = ts });
        }
        
        public virtual void SendAllSessionFinishedMessage(List<string> sessionUids, long ts)
        {
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionsFinished { ids = sessionUids, ts = ts });
        }
        
        public virtual void SendSessionCancelledMessage(string uid, long ts)
        {
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionCancelled { sessionId = uid, ts = ts });
        }

        public virtual void SendAllSessionCancelledMessage(List<string> uids, long ts)
        {
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionsCancelled { ids = uids, ts = ts });
        }

        /// <summary>
        /// Send coverage data to the admin part ("COVERAGE_DATA_PART")
        /// </summary>
        public virtual void SendCoverageData(string sessionUid, List<ExecClassData> data)
        {
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new CoverDataPart { data = data, sessionId = sessionUid });
        }
        
        public virtual void SendSessionChangedMessage(string sessionUid, int probeCount)
        {
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionChanged { sessionId = sessionUid, probeCount = probeCount });
        }
        #endregion
        #region Send API
        #region Send
        public void Send(string topic, AbstractMessage message)
        {
            SendConcrete(message.type, topic, Serialize(message));
        }
        
        protected abstract void SendConcrete(string messageType, string topic, string message);
        #endregion
        #region SendToPlugin
        public void SendToPlugin(string topic, AbstractMessage message)
        {
            SendToPluginConcrete(topic, Serialize(message));
        }

        protected abstract void SendToPluginConcrete(string topic, string message);
        #endregion

        protected abstract string Serialize(object message);
        
        #region Test sendings
        public virtual void SendOutgoingTest(OutgoingMessage data)
        {
        }
        
        public virtual void SendOutgoingTest(string topic, OutgoingMessage data)
        {
        }
        
        public virtual void SendIncomingTest(string topic, IncomingMessage message)
        {
        }
        #endregion
        #endregion
    }
}