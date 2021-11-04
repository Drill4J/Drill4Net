using System;
using Drill4Net.Configuration;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Agent's options
    /// </summary>
    [Serializable]
    public class AgentOptions : TargetOptions
    {
        /// <summary>
        /// Options for the communicating between Agent part of instrumented App and the admin side
        /// </summary>
        public DrillServerOptions Admin { get; set; }

        /// <summary>
        /// Path where Transmitter plugins are located (contexters)
        /// </summary>
        public string PluginDir { get; set; }

        /// <summary>
        /// Auxiliary options for connector subsystem (Drill native librarry for websocket communications)
        /// </summary>
        public ConnectorAuxOptions Connector { get; set; }
    }
}
