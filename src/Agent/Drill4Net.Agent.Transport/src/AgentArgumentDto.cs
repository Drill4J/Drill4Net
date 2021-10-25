using System;

namespace Drill4Net.Agent.Transport
{
    /// <summary>
    /// Data structure for init agent on Drill Admin side
    /// (websocket connection, agent metadata, log level, etc)
    /// </summary>
    public record AgentArgumentDto
    {
#pragma warning disable IDE1006 // Naming Styles
        public string agentId { get; set; }

        public string adminAddress { get; set; }
        public string buildVersion { get; set; } = "unspecified";
        public string agentVersion { get; set; } = "";
        public string groupId { get; set; } = "";
        public string instanceId { get; set; } = Guid.NewGuid().ToString();
        public string logLevel { get; set; } = nameof(ConnectorLogLevel.ERROR);
        public string logFile { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
