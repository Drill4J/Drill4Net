﻿using System;
using Newtonsoft.Json;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;

/* About MS standard serializer
 * DO NOT USE $MS standard serializer from System.Text.Json 
 * because it will fail to resolve in some cases (project is NetStandard 2.0 now)
 */

namespace Drill4Net.Agent.Transport
{
    /// <summary>
    /// Receiver of data from admin side
    /// </summary>
    public class AgentReceiver : IAgentReceiver
    {
        #region Events
        /// <summary>
        /// New scope data is initialized
        /// </summary>
        public event InitScopeDataHandler InitScopeData;

        public event PluginLoadedHandler PluginLoaded;

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

        private readonly Connector _connector;
        private readonly JsonSerializerSettings _deserOpts; //JsonSerializerOptions
        private readonly Logger _logger;

        /*******************************************************************************/

        public AgentReceiver(Connector connector)
        {
            _logger = new TypedLogger<AgentReceiver>(CoreConstants.SUBSYSTEM_AGENT);

            //_deserOpts = new JsonSerializerOptions
            //{
            //    AllowTrailingCommas = true,
            //    PropertyNameCaseInsensitive = true,
            //};

            _deserOpts = new JsonSerializerSettings
            {
            };

            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _connector.MessageReceived += MessageReceived;
        }

        /*******************************************************************************/

        /// <summary>
        /// Messages received from the Admin side.
        /// </summary>
        /// <param name="topic">The topic.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="Exception">Handlers are not attached for received events</exception>
        protected virtual void MessageReceived(string topic, string message)
        {
            if (RequestClassesData == null)
                throw new Exception("Handlers are not attached for received events");
            _logger.Debug($"Message received: topic={topic}, message={message}");
            //
            try
            {
                switch (topic)
                {
                    case AgentConstants.TOPIC_HEADER_CHANGE: //we don't work with it yet (global session mapping?)
                        break;
                    case AgentConstants.TOPIC_AGENT_NAMESPACES: //TODO: additional filter for incoming probes?
                        break;
                    case AgentConstants.TOPIC_AGENT_PLUGIN_LOAD: // Agent init - if it has the Fast initializing
                        _logger.Debug($"Plugin loaded: [{message}]");
                        PluginLoaded?.Invoke();
                        break;
                    case AgentConstants.TOPIC_CLASSES_LOAD:
                        _logger.Debug("Request classes");
                        RequestClassesData?.Invoke();
                        break;
                    case AgentConstants.TOPIC_TOGGLE_PLUGIN: // as Slow initializing (with agent registration)
                        var plugin = message; //TODO: processing
                        _logger.Debug($"Plugin toggled: [{plugin}]");
                        TogglePlugin?.Invoke(plugin);
                        break;
                    case AgentConstants.TOPIC_PLUGIN_ACTION:
                        message = message.Substring(message.IndexOf('{')); //crunch: bug in messages on admin side
                        var baseInfo = Deserialize<IncomingMessage>(message);
                        _logger.Debug($"Plugin action: {baseInfo.type}");
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
                            case AgentConstants.MESSAGE_IN_STOP_ALL:
                                StopAllSessions?.Invoke();
                                break;
                            case AgentConstants.MESSAGE_IN_CANCEL_SESSION:
                                var cancelInfo = Deserialize<CancelAgentSession>(message);
                                CancelSession?.Invoke(cancelInfo);
                                break;
                            case AgentConstants.MESSAGE_IN_CANCEL_ALL:
                                CancelAllSessions?.Invoke();
                                break;
                            case AgentConstants.MESSAGE_IN_ADD_SESSION_TESTS: //as ack
                                break;
                            default:
                                _logger.Error($"Unknown message type for {nameof(AgentConstants.TOPIC_PLUGIN_ACTION)}: [{baseInfo.type}]\nMessage:\n{message}");
                                break;
                        }
                        break;
                    default:
                        _logger.Error($"Unknown topic [{topic}]");
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.Error("Message receiving error", e);
            }
        }

        internal T Deserialize<T>(string obj) where T : class, new()
        {
            return JsonConvert.DeserializeObject<T>(obj, _deserOpts); // JsonSerializer.Deserialize<T>(obj, _deserOpts);
        }
    }
}