using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transport
{
    //Delegates are marshalled directly. The only thing you need to take care of is the “calling convention”.
    //The default calling convention is WinApi (which equals to StdCall on Windows).
    //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ReceivedMessageHandler(string topic, string message);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    [SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
    public class Connector
    {
        public event ReceivedMessageHandler MessageReceived;

        [DllImport("agent_connector")]
        static extern int agent_connector_symbols();

        [DllImport("agent_connector")]
        static extern void initialize_agent(string agentId, string adminAddress, string buildVersion,
                                            string groupId, string instanceId, ReceivedMessageHandler received);

        [DllImport("agent_connector")]
        static extern int sendMessage(string messageType, string destination, string content);

        [DllImport("agent_connector")]
        static extern int sendPluginMessage(string pluginId, string content);

        private ReceivedMessageHandler _received; //it's needed to prevent GC collecting

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