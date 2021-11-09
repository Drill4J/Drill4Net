using System;
using Drill4Net.Configuration;

namespace Drill4Net.Agent.Abstract
{
    [Serializable]
    public class AgentDebugOptions : IDebugOptions
    {
        public bool Disabled { get; set; }

        /// <summary>
        /// Write trace probe data to the file probes.log in Log directory
        /// </summary>
        public bool WriteProbes { get; set; }
    }
}
