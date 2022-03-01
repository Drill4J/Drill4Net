using System;
using Drill4Net.Common;
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
        /// Path where Transmitter (as agent) plugins are located (contexters)
        /// </summary>
        public string PluginDir { get; set; }

        /// <summary>
        /// If needed to automatic creation session on admin side
        /// (for example, for testing the class Drill4Net.Target.Common.ModelTarget/>)
        /// </summary>
        public bool CreateManualSession { get; set; }

        /// <summary>
        /// Auxiliary options for connector subsystem (Drill native librarry for websocket communications)
        /// </summary>
        public ConnectorAuxOptions Connector { get; set; }

        public AgentDebugOptions Debug { get; set; }

        /**************************************************************************/

        public AgentOptions()
        {
            Type = CoreConstants.SUBSYSTEM_AGENT;
        }
    }
}
