using System;
using System.Text.Json;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Transport
{
    /// <summary>
    /// Receiver of data from admin side
    /// </summary>
    public class AgentReceiver : IReceiver
    {
        #region Events
        /// <summary>
        /// Session is started on admin side
        /// </summary>
        public event StartSessionHandler StartSession;

        /// <summary>
        /// Session must stopped
        /// </summary>
        public event StopSessionHandler StopSession;

        /// <summary>
        /// All session must stopped
        /// </summary>
        public event StopAllSessionsHandler StopAllSessions;

        /// <summary>
        /// Session is cancelled on admin side
        /// </summary>
        public event CancelSessionHandler CancelSession;

        /// <summary>
        /// All sessions are cancelled on admin side
        /// </summary>
        public event CancelAllSessionsHandler CancelAllSessions;
        #endregion

        private readonly Connector _connector;

        /************************************************************************/

        public AgentReceiver(Connector receiver)
        {
            _connector = receiver ?? throw new ArgumentNullException(nameof(_connector));
        }

        /************************************************************************/

        public virtual void MessageReceived(string topic, string message)
        {
            try
            {
                switch (topic)
                {
                    case AgentConstants.MESSAGE_IN_START_SESSION:
                        var startInfo = Deserialize<StartAgentSession>(message);
                        StartSession?.Invoke(startInfo);
                        break;
                    case AgentConstants.MESSAGE_IN_STOP_SESSION:
                        var stopInfo = Deserialize<StopAgentSession>(message);
                        StopSession?.Invoke(stopInfo);
                        break;
                    case AgentConstants.MESSAGE_IN_STOP_ALL: //in fact
                        StopAllSessions?.Invoke();
                        break;
                    case AgentConstants.MESSAGE_IN_CANCEL_SESSION:
                        var cancelInfo = Deserialize<CancelAgentSession>(message);
                        CancelSession?.Invoke(cancelInfo);
                        break;
                    case AgentConstants.MESSAGE_IN_CANCEL_ALL: //in fact
                        CancelAllSessions?.Invoke();
                        break;
                    default:
                        //log
                        break;
                }
            }
            catch (Exception e)
            {
                //log
            }
        }

        internal T Deserialize<T>(string obj) where T : class, new()
        {
            return JsonSerializer.Deserialize<T>(obj);
        }
    }
}