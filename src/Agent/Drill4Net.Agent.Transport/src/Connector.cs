﻿using System.Runtime.InteropServices;
using System.Threading;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transport
{
    public delegate void ReceivedMessageHandler(string topic, string message);

    public class Connector
    {
        [DllImport("agent_connector")]
        extern static int initialize_agent(string agentId, string adminAddress, string buildVersion,
                                           string groupId, string instanceId, ReceivedMessageHandler received);

        [DllImport("agent_connector")]
        extern static int sendMessage(string messageType, string destination, string content);

        [DllImport("agent_connector")]
        extern static int sendPluginMessage(string pluginId, string content);

        /***********************************************************************************/

        public void Connect(string url, AgentPartConfig agentCfg, ReceivedMessageHandler received)
        {
            initialize_agent(
                agentCfg.Id,
                url, //"localhost:8090",
                agentCfg.BuildVersion,
                agentCfg.ServiceGroupId,
                agentCfg.InstanceId,
                received);
            Thread.Sleep(2000);
        }

        public void SendMessage(string messageType, string topic, string message)
        {
            sendMessage(messageType, topic, message);
        }

        public void SendPluginMessage(string pluginId, string message)
        {
            sendPluginMessage(pluginId, message); //pluginId = "test2code"
        }
    }
}
