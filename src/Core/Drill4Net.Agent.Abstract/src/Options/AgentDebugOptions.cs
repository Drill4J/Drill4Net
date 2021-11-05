using Drill4Net.Configuration;

namespace Drill4Net.Agent.Abstract
{
    public class AgentDebugOptions : IDebugOptions
    {
        public bool Disabled { get; set; }

        /// <summary>
        /// Write trace probe data to the file probes.log in Log directory
        /// </summary>
        public bool WriteProbes { get; set; }
    }
}
