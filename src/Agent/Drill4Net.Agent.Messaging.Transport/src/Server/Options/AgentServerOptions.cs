using System.Collections.Generic;

namespace Drill4Net.Agent.Messaging.Transport
{
    public class AgentServerOptions : MessagerOptions
    {
        /// <summary>
        /// Gets or sets the agent potential worker dirs.
        /// </summary>
        /// <value>
        /// The agent worker path.
        /// </value>
        public List<string> WorkerDirs { get; set; }
        
        public AgentServerDebugOptions Debug { get; set; }
    }
}
