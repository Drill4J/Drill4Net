using System.Collections.Generic;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    public abstract class AbstractSender : ISender
    {
        #region Send messages
        #region Init
        /// <summary>
        /// "Agent is starting init process" message ("INIT")
        /// </summary>
        public virtual void SendInitMessage(int classesCount)
        {
            var mess = new InitInfo(classesCount);
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, mess);
        }

        /// <summary>
        /// "Agent is initialized" message ("INITIALIZED")
        /// </summary>
        public virtual void SendInitializedMessage()
        {
            var mess = new Initialized {Msg = "Initialized"}; //can be any string
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, mess);
        }

        /// <summary>
        /// "INIT_DATA_PART"
        /// </summary>
        /// <param name="entities"></param>
        public virtual void SendClassesDataMessage(List<AstEntity> entities)
        {
            var mess = new InitDataPart {AstEntities = entities};
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, mess);
        }
        #endregion

        public virtual void SendSessionStartedMessage(string sessionUid, long ts)
        {
            var mess = new SessionStarted 
            { 
                SessionId  = sessionUid, 
                Ts = ts,
                //IsRealtime = ...
                //TestType = ...
            };
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, mess);
        }
        
        public virtual void SendSessionFinishedMessage(string sessionUid, long ts)
        {
            var mess = new SessionFinished { SessionId = sessionUid, Ts = ts };
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, mess);
        }
        
        public virtual void SendAllSessionFinishedMessage(List<string> sessionUids, long ts)
        {
            var mess = new SessionsFinished { IDs = sessionUids, Ts = ts };
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, mess);
        }
        
        public virtual void SendSessionCancelledMessage(string uid, long ts)
        {
            var mess = new SessionCancelled { SessionId  = uid, Ts = ts };
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, mess);
        }

        public virtual void SendAllSessionCancelledMessage(List<string> uids, long ts)
        {
            var mess = new SessionsCancelled { IDs = uids, Ts = ts };
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, mess);
        }

        /// <summary>
        /// Send coverage data to the admin part ("COVERAGE_DATA_PART")
        /// </summary>
        public virtual void SendCoverageData(string sessionUid, List<ExecClassData> data)
        {
            var mess = new CoverDataPart { Data = data, SessionId = sessionUid };
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, mess);
        }
        
        public virtual void SendSessionChangedMessage(string sessionUid, int probeCount)
        {
            var mess = new SessionChanged { SessionId  = sessionUid, ProbeCount = probeCount };
            SendToPlugin(AgentConstants.ADMIN_PLUGIN_NAME, mess);
        }
        #endregion
        #region Send API
        #region Send
        public void Send(string topic, AbstractMessage message)
        {
            SendConcrete(message.Type, topic, Serialize(message));
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
        
        public virtual void SendTest(AbstractMessage data)
        {
        }
        
        public virtual void SendTest(IncomingMessage message)
        {
        }
        #endregion
    }
}