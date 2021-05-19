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
        /// New scope data is initialized
        /// </summary>
        public event InitScopeDataHandler InitScopeData;

        /// <summary>
        /// Admin side requests the classes metadata
        /// </summary>
        public event RequestClassesDataHandler RequestClassesData;

        /// <summary>
        /// On admin side toggled some plugin
        /// </summary>
        public event TogglePluginHandler TogglePlugin;

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

        private readonly JsonSerializerOptions _deserOpts;

        /************************************************************************/

        public AgentReceiver(Connector receiver)
        {
            _deserOpts = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
            };

            var connector = receiver ?? throw new ArgumentNullException(nameof(receiver));
            connector.MessageReceived += MessageReceived;
        }

        /************************************************************************/

        protected virtual void MessageReceived(string topic, string message)
        {
            if (RequestClassesData == null)
                throw new Exception("Handlers are not attached for received events");
            //
            try
            {
                switch (topic)
                {
                    case AgentConstants.TOPIC_HEADER_CHANGE: //we don't work with it yet (global session mapping?)
                        break;
                    case AgentConstants.TOPIC_AGENT_NAMESPACES: //TODO: additional filter for incoming probes?
                        break;
                    case AgentConstants.TOPIC_AGENT_LOAD: //needed?
                        break;
                    case AgentConstants.TOPIC_CLASSES_LOAD:
                        RequestClassesData?.Invoke();
                        break;
                    case AgentConstants.TOPIC_TOGGLE_PLUGIN:
                        var plugin = message; //TODO: processing
                        TogglePlugin?.Invoke(plugin);
                        break;
                    case AgentConstants.TOPIC_PLUGIN_ACTION:
                        message = message.Substring(message.IndexOf('{')); //crunch: bug in messages on admin side
                        var baseInfo = Deserialize<IncomingMessage>(message);
                        switch (baseInfo.type)
                        {
                            case AgentConstants.MESSAGE_IN_INIT_ACTIVE_SCOPE:
                                var scope = Deserialize<InitActiveScope>(message);
                                InitScopeData?.Invoke(scope);
                                break;
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
                        break;
                    default:
                        //log
                        break;
                }
            }
            catch (Exception e)
            {
                //TODO: log!!!
            }
        }

        internal T Deserialize<T>(string obj) where T : class, new()
        {
            return JsonSerializer.Deserialize<T>(obj, _deserOpts);
        }
    }
}