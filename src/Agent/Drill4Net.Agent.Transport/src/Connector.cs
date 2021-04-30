﻿using System;
using System.Runtime.InteropServices;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transport
{
    //Delegates are marshalled directly. The only thing you need to take care of is the “calling convention”.
    //The default calling convention is Winapi (which equals to StdCall on Windows).
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ReceivedMessageHandler(string topic, string message);

    public class Connector
    {
        public event ReceivedMessageHandler MessageReceived;

        [DllImport("agent_connector")]
        extern static int agent_connector_symbols();

        [DllImport("agent_connector"/*, CallingConvention = CallingConvention.Cdecl*/)]
        extern static void initialize_agent(string agentId, string adminAddress, string buildVersion,
                                            string groupId, string instanceId,
                                            [MarshalAs(UnmanagedType.FunctionPtr)]
                                            ReceivedMessageHandler received);

        [DllImport("agent_connector")]
        extern static int sendMessage(string messageType, string destination, string content);

        [DllImport("agent_connector")]
        extern static int sendPluginMessage(string pluginId, string content);

        private ReceivedMessageHandler _received; //it's needed for prevent GC colleting

        /***********************************************************************************/

        public void Connect(string url, AgentPartConfig agentCfg)
        {
            _received = ReceivedMessageHandler;

            initialize_agent(
                agentCfg.Id,
                url, //"localhost:8090",
                agentCfg.BuildVersion,
                agentCfg.ServiceGroupId,
                agentCfg.InstanceId,
                _received);
        }

        [AllowReversePInvokeCalls]
        private void ReceivedMessageHandler(string topic, string message)
        {
            MessageReceived?.Invoke(topic, message);
        }

        public void SendMessage(string messageType, string topic, string message)
        {
            sendMessage(messageType, topic, message);
        }

        public void SendPluginMessage(string pluginId, string message)
        {
            sendPluginMessage(pluginId, message); //now pluginId = "test2code"
        }
    }
}