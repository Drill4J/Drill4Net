using System.Collections.Generic;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    public abstract class AbstractSender : ISender
    {
        #region Send messages
        /// <summary>
        /// "Agent is initialized" message ("INITIALIZED")
        /// </summary>
        public virtual void SendInitializedMessage()
        {
            var mess = new Initialized {Msg = "Initialized"}; //can be any string
            Send(mess);
        }

        /// <summary>
        /// "INIT_DATA_PART"
        /// </summary>
        /// <param name="entities"></param>
        public virtual void SendClassesDataMessage(List<AstEntity> entities)
        {
            var mess = new InitDataPart {AstEntities = entities};
            Send(mess);
        }
        
        public virtual void SendSessionStartedMessage(string sessionUid, long ts)
        {
            var mess = new SessionStarted 
            { 
                SessionId  = sessionUid, 
                Ts = ts,
                //IsRealtime = ...
                //TestType = ...
            };
            Send(mess);
        }
        
        public virtual void SendSessionFinishedMessage(string sessionUid, long ts)
        {
            var mess = new SessionFinished { SessionId = sessionUid, Ts = ts };
            Send(mess);
        }
        
        public virtual void SendAllSessionFinishedMessage(List<string> sessionUids, long ts)
        {
            var mess = new SessionsFinished { IDs = sessionUids, Ts = ts };
            Send(mess);
        }
        
        public virtual void SendSessionCancelledMessage(string uid, long ts)
        {
            var mess = new SessionCancelled { SessionId  = uid, Ts = ts };
            Send(mess);
        }

        public virtual void SendAllSessionCancelledMessage(List<string> uids, long ts)
        {
            var mess = new SessionsCancelled { IDs = uids, Ts = ts };
            Send(mess);
        }

        /// <summary>
        /// Send coverage data to the admin part ("COVERAGE_DATA_PART")
        /// </summary>
        public virtual void SendCoverageData(string sessionUid, List<ExecClassData> data)
        {
            var mess = new CoverDataPart { Data = data, SessionId = sessionUid };
            Send(mess);
        }
        
        public virtual void SendSessionChangedMessage(string sessionUid, int probeCount)
        {
            var mess = new SessionChanged { SessionId  = sessionUid, ProbeCount = probeCount };
            Send(mess);
        }
        #endregion
        #region Send API
        public void Send(AbstractOutgoingMessage message)
        {
            SendConcrete(Serialize(message));
        }
        
        protected abstract void SendConcrete(string data);

        protected abstract string Serialize(object message);
        
        public virtual void SendTest(AbstractOutgoingMessage data)
        {
        }
        
        public virtual void SendTest(IncomingMessage message)
        {
        }
        #endregion
    }
}