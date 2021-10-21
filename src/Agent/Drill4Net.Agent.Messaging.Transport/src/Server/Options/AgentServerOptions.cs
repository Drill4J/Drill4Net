namespace Drill4Net.Agent.Messaging.Transport
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
        
        public AgentServerDebugOptions Debug { get; set; }
    }
}
