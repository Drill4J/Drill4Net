using Drill4Net.Agent.Messaging.Transport;

namespace Drill4Net.Agent.Service
{
    public class AgentServerOptions : MessageReceiverOptions
    {
        /// <summary>
        /// Gets or sets the agent worker path.
        /// </summary>
        /// <value>
        /// The agent worker path.
        /// </value>
        public string WorkerPath { get; set; }
    }
}
