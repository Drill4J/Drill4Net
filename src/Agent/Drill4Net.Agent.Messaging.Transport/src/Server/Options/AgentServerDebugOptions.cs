using Drill4Net.Configuration;

namespace Drill4Net.Agent.Messaging.Transport
{
    /// <summary>
    /// Options for debug the Agent Server
    /// </summary>
    public class AgentServerDebugOptions : IDebugOptions
    {
        /// <summary>
        /// Is Debug mode is active?
        /// </summary>
        /// <value>
        ///   <c>true</c> if disabled; otherwise, <c>false</c>.
        /// </value>
        public bool Disabled { get; set; }

        public bool DontStartWorker { get; set; }
        public bool DontDeleteTopics { get; set; }
    }
}
