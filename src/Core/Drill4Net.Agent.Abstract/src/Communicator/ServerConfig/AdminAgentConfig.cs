using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Drill4Net.Common;

namespace Drill4Net.Agent.Abstract
{
    //https://kb.epam.com/pages/viewpage.action?pageId=986565061

    /// <summary>
    /// Config for admin side about Agent and instrumented application instances,
    /// applied during connect (header 'AgentConfig' for WebSocket handshake).
    /// </summary>
    public class AdminAgentConfig
    {
        /// <summary>
        /// Some ID or Name of instrumented App
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Instance ID of current Agent unit (to distinguish between different agent 
        /// instances that collect data from the same instrumented application instance, 
        /// for example, for load balancing purposes)
        /// </summary>
        public string InstanceId { get; set; }

        /// <summary>
        /// Build version of instrumented application instance
        /// </summary>
        public string BuildVersion { get; set; }

        /// <summary>
        /// Agent Version
        /// </summary>
        public string AgentVersion { get; set; }

        /// <summary>
        /// Service group Id
        /// </summary>
        public string ServiceGroupId { get; set; }

        /// <summary>
        /// Always 'DOTNET'
        /// </summary>
        public string AgentType { get; set; }

        public LogLevel ConnectorLogLevel { get; set; }
        public string ConnectorLogFilePath { get; set; }

        /************************************************************************************/

        public AdminAgentConfig(string appId, string appVersion, string agentVersion)
        {
            //App
            Id = appId;
            InstanceId = Guid.NewGuid().ToString();
            BuildVersion = string.IsNullOrWhiteSpace(appVersion) ? "0.0.0" : appVersion;

            //Agent
            AgentType = "DOTNET";
            ServiceGroupId = "";
            AgentVersion = agentVersion;

            //Connector
            ConnectorLogLevel = LogLevel.Error;
            ConnectorLogFilePath = AbstractAgent.GetDefaultConnectorLogFilePath();
        }
    }
}
