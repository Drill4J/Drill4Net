using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Websocket.Client;
using Websocket.Client.Models;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Messages;
using Drill4Net.Agent.Abstract.Transfer;
using Newtonsoft.Json;

namespace Drill4Net.Agent.Transport
{
    /// <summary>
    /// Receiver of data from admin side
    /// </summary>
    public class AgentReceiver : AbstractReceiver
    {
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
                var mess = JsonConvert.DeserializeObject<ConnectorQueueItem>(message.Text);
                var payload = mess?.Message;
                switch (mess?.Destination)
                {
                    case AgentConstants.MESSAGE_IN_START_SESSION:
                        var startInfo = Deserialize<StartAgentSession>(payload);
                        
                        break;
                    case AgentConstants.MESSAGE_IN_STOP_SESSION:
                        var stopInfo = Deserialize<StopAgentSession>(payload);
                        
                        break;
                    case AgentConstants.MESSAGE_IN_STOP_ALL: //in fact
                        
                        break;
                    case AgentConstants.MESSAGE_IN_CANCEL_SESSION:
                        var cancelInfo = Deserialize<CancelAgentSession>(payload);
                        
                        break;
                    case AgentConstants.MESSAGE_IN_CANCEL_ALL: //in fact
                        
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
            return JsonConvert.DeserializeObject<T>(obj);
        }

        private void ReconnectHandler(ReconnectionInfo info)
        {
            //need resubscribe??? 
        }
    }
}