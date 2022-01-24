using System.Web;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transport
{
    //https://drill4j.jfrog.io/ui/repos/tree/General/drill%2Fcom%2Fepam%2Fdrill%2Fdotnet%2Fagent_connector-mingwX64-debug%2F0.5.3

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
        static extern void initialize_agent(string args, ReceivedMessageHandler received);

        //it is used on our agent to send messages that are not related to the plugin:
        //this is setting up a loglevel, sending packages, etc. You hardly need it yet
        [DllImport("agent_connector")]
        static extern int sendMessage(string messageType, string destination, string content);

        [DllImport("agent_connector")]
        static extern int sendPluginMessage(string pluginId, string content);

        [DllImport("agent_connector")]
        static extern int sendPluginAction(string pluginId, string content);

        [DllImport("agent_connector")]
        static extern int addTests(string pluginId, string testsRun);

        [DllImport("agent_connector")]
        static extern void startSession(string pluginId, string sessionId, bool isRealtime, bool isGlobal);

        [DllImport("agent_connector")]
        static extern void stopSession(string pluginId, string sessionId);

        private ReceivedMessageHandler _received; //it's needed to prevent GC collecting

        /***********************************************************************************/

        public void Connect(string url, AdminAgentConfig agentCfg)
        {
            _received = ReceivedMessageHandler;

            var agentConnOpts = new AgentArgumentDto
            {
                agentId = agentCfg.Id,
                adminAddress = url,
                buildVersion = HttpUtility.UrlEncode(agentCfg.BuildVersion),
                agentVersion = HttpUtility.UrlEncode(agentCfg.AgentVersion),
                instanceId = agentCfg.InstanceId,
                groupId = agentCfg.ServiceGroupId,
                logLevel = ConvertToConnectorLogLevel(agentCfg.ConnectorLogLevel).ToString(),
                logFile = agentCfg.ConnectorLogFilePath,
            };
            var cfgStr = JsonConvert.SerializeObject(agentConnOpts);

            initialize_agent(cfgStr, _received);
        }

        private ConnectorLogLevel ConvertToConnectorLogLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => ConnectorLogLevel.DEBUG,
                LogLevel.Information => ConnectorLogLevel.INFO,
                LogLevel.Warning => ConnectorLogLevel.WARN,
                LogLevel.Error => ConnectorLogLevel.ERROR,
                LogLevel.Critical => ConnectorLogLevel.ERROR,
                LogLevel.None => ConnectorLogLevel.ERROR,
                _ => ConnectorLogLevel.TRACE,
            };
        }

        [AllowReversePInvokeCalls]
        private void ReceivedMessageHandler(string topic, string message)
        {
            MessageReceived?.Invoke(topic, message);
        }

        /// <summary>
        /// It is used on our agent to send messages that are not related to the plugin: 
        /// this is setting up a loglevel, sending packages, etc. You hardly need it yet
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="route"></param>
        /// <param name="message"></param>
        public void SendMessage(string messageType, string route, string message)
        {
            sendMessage(messageType, route, message);
        }

        /// <summary>
        /// Send message to certain plugin
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="message"></param>
        public void SendPluginMessage(string pluginId, string message)
        {
            sendPluginMessage(pluginId, message); //currently pluginId is the only one = "test2code"
        }

        /// <summary>
        /// Send action for plugin
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="message"></param>
        public void SendPluginAction(string pluginId, string message)
        {
            sendPluginAction(pluginId, message); //currently pluginId is the only one = "test2code"
        }

        /// <summary>
        /// Start the session on Drill Admin side
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="sessionId"></param>
        /// <param name="isRealtime"></param>
        /// <param name="isGlobal"></param>
        public void StartSession(string pluginId, string sessionId, bool isRealtime, bool isGlobal)
        {
            startSession(pluginId, sessionId, isRealtime, isGlobal);
        }

        /// <summary>
        /// Stop the session on Drill Admin side
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="sessionId"></param>
        public void StopSession(string pluginId, string sessionId)
        {
            stopSession(pluginId, sessionId);
        }

        /// <summary>
        /// Add info about running tests.
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="tests2Run"></param>
        public void AddTestsRun(string pluginId, string tests2Run)
        {
            addTests(pluginId, tests2Run);
        }
    }
}