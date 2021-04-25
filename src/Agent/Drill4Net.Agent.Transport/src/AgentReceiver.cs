﻿using System;
using Newtonsoft.Json;
using Websocket.Client;
using Websocket.Client.Models;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Messages;
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

        private readonly WebsocketClient _receiver;

        /************************************************************************/

        public AgentReceiver(WebsocketClient receiver)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            _receiver.ReconnectionHappened.Subscribe(info => ReconnectHandler(info));
            _receiver.MessageReceived.Subscribe(msg => ReceivedHandler(msg));
            _receiver.Start();
        }

        /************************************************************************/

        private void ReceivedHandler(ResponseMessage message)
        {
            try
            {
                // var mess = JsonConvert.DeserializeObject<ConnectorQueueItem>(message.Text);
                // var payload = mess?.Message;
                // switch (mess?.Destination)
                // {
                //     case AgentConstants.MESSAGE_IN_START_SESSION:
                //         var startInfo = Deserialize<StartAgentSession>(payload);
                //         StartSession?.Invoke(startInfo);
                //         break;
                //     case AgentConstants.MESSAGE_IN_STOP_SESSION:
                //         var stopInfo = Deserialize<StopAgentSession>(payload);
                //         StopSession?.Invoke(stopInfo);
                //         break;
                //     case AgentConstants.MESSAGE_IN_STOP_ALL: //in fact
                //         StopAllSessions?.Invoke();
                //         break;
                //     case AgentConstants.MESSAGE_IN_CANCEL_SESSION:
                //         var cancelInfo = Deserialize<CancelAgentSession>(payload);
                //         CancelSession?.Invoke(cancelInfo);
                //         break;
                //     case AgentConstants.MESSAGE_IN_CANCEL_ALL: //in fact
                //         CancelAllSessions?.Invoke();
                //         break;
                //     default:
                //         //log
                //         break;
                //}
                
                var mess = JsonConvert.DeserializeObject<IncomingMessage>(message.Text);
            }
            catch (Exception e)
            {
                //log
            }
        }

        internal T Deserialize<T>(string obj) where T : class, new()
        {
            return JsonConvert.DeserializeObject<T>(obj);
        }

        private void ReconnectHandler(ReconnectionInfo info)
        {
            //need resubscribe??? 
        }
    }
}