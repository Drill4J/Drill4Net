using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184

    public abstract class AbstractSender : ISender
    {
        #region Send messages
        /// <summary>
        /// "Agent is initialized" message ("INITIALIZED")
        /// </summary>
        public virtual void SendInitializedMessage()
        {
            Send(AgentConstants.MESSAGE_OUT_INITIALIZED, "Initialized"); //can be any string
        }

        /// <summary>
        /// "INIT_DATA_PART"
        /// </summary>
        /// <param name="entities"></param>
        public virtual void SendClassesDataMessage(IEnumerable<AstEntity> entities)
        {
            Send(AgentConstants.MESSAGE_OUT_INIT_DATA_PART, entities);
        }
        
        public virtual void SendSessionStartedMessage(string sessionUid, long ts)
        {
            Send(AgentConstants.MESSAGE_OUT_SESSION_STARTED, sessionUid);
        }

        /// <summary>
        /// Send coverage data to the admin part ("COVERAGE_DATA_PART")
        /// </summary>
        public virtual void SendCoverageData(List<ExecClassData> data)
        {
            Send(AgentConstants.MESSAGE_OUT_COVERAGE_DATA_PART, data);
        }
        
        public virtual void SendSessionChangedMessage(string sessionUid, long ts)
        {
            Send(AgentConstants.MESSAGE_OUT_SESSION_CHANGED, sessionUid);
        }
        
        public virtual void SendSessionCancelledMessage(string sessionUid, long ts)
        {
            Send(AgentConstants.MESSAGE_OUT_SESSION_CANCELLED, sessionUid);
        }
        
        public virtual void SendAllSessionsCancelledMessage(List<string> sessionUids, long ts)
        {
            Send(AgentConstants.MESSAGE_OUT_SESSION_ALL_CANCELLED, null);
        }        

        public virtual void SendSessionFinishedMessage(string sessionUid, long ts)
        {
            Send(AgentConstants.MESSAGE_OUT_SESSION_FINISHED, sessionUid);
        }
        
        public virtual void SendAllSessionFinishedMessage(List<string> sessionUids, long ts)
        {
            Send(AgentConstants.MESSAGE_OUT_SESSION_ALL_FINISHED, null);
        }
        #endregion
        #region Send API
        public void Send(string messageType, object message)
        {
            SendConcrete(ConvertToPayload(messageType, message));
        }

        protected abstract void SendConcrete(string payload);

        protected abstract string ConvertToPayload(string messageType, object message);
        
        public virtual void SendTest(string messageType, object data)
        {
        }
        #endregion
    }
}