using System.Collections.Generic;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    public abstract class AbstractCoverageSender : IAgentCoverageSender
    {
        private readonly Logger _logger;

        /********************************************************************************/

        protected AbstractCoverageSender(string subsystem)
        {
            _logger = new TypedLogger<AbstractCoverageSender>(subsystem);
        }

        /********************************************************************************/

        #region Send messages
        #region Init
        /// <summary>
        /// "Send Scope Initialized" message ("SCOPE_INITIALIZED")
        /// </summary>
        public virtual void SendScopeInitialized(InitActiveScope scope, long ts)
        {
            _logger.Debug("Send ScopeInitialized");
            var load = scope.Payload;
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new ScopeInitialized(load.Id, load.Name, load.PrevId, ts));
        }
        
        /// <summary>
        /// "Agent is starting init process" message ("INIT")
        /// </summary>
        public virtual void SendInitMessage(int classesCount)
        {
            _logger.Debug($"Send init message. Class count={classesCount}");
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, new InitInfo(classesCount));
        }

        /// <summary>
        /// "INIT_DATA_PART"
        /// </summary>
        /// <param name="entities"></param>
        public virtual void SendClassesDataMessage(List<AstEntity> entities)
        {
            _logger.Debug($"Send classes data. Count={entities?.Count}");
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, new InitDataPart { astEntities = entities });
        }

        /// <summary>
        /// "Agent is initialized" message ("INITIALIZED")
        /// </summary>
        public virtual void SendInitializedMessage()
        {
            _logger.Debug("Send Initialized message");
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, new Initialized { msg = "Initialized" }); //can be any string
        }
        #endregion

        public virtual void SendSessionStartedMessage(string sessionUid, string testType, bool isRealTime, long ts)
        {
            var data = new SessionStarted
            {
                sessionId = sessionUid,
                isRealtime = isRealTime,
                testType = testType,
                ts = ts,
            };
            _logger.Debug($"Send SessionStarted message. {data}");
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, data);
        }

        public virtual void SendSessionFinishedMessage(string sessionUid, long ts)
        {
            _logger.Debug($"Send SessionFinished message. Session={sessionUid}");
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionFinished { sessionId = sessionUid, ts = ts });
        }

        public virtual void SendAllSessionFinishedMessage(List<string> sessionUids, long ts)
        {
            _logger.Debug($"Send AllSessionFinished message. Sessions={string.Join(",", sessionUids)}");
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionsFinished { ids = sessionUids, ts = ts });
        }

        public virtual void SendSessionCancelledMessage(string uid, long ts)
        {
            _logger.Debug($"Send SessionCancelled message. Session={uid}");
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionCancelled { sessionId = uid, ts = ts });
        }

        public virtual void SendAllSessionCancelledMessage(List<string> uids, long ts)
        {
            _logger.Debug($"Send AllSessionCancelled message. Sessions={string.Join(",", uids)}");
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionsCancelled { ids = uids, ts = ts });
        }

        /// <summary>
        /// Send coverage data to the admin part ("COVERAGE_DATA_PART")
        /// </summary>
        public virtual void SendCoverageData(string sessionUid, List<ExecClassData> data)
        {
            //no logging here!
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new CoverDataPart { data = data, sessionId = sessionUid });
        }
        
        public virtual void SendSessionChangedMessage(string sessionUid, int probeCount)
        {
            //no logging here!
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME,
                new SessionChanged { sessionId = sessionUid, probeCount = probeCount });
        }

        public virtual void SendStartSessionCommand(string name)
        {

        }

        public virtual void SendStopSessionCommand(string name)
        {

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