using Drill4Net.Configuration;
using System;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Agent's options
    /// </summary>
    [Serializable]
    public class AgentOptions : BaseTargetOptions
    {
        /// <summary>
        /// Options for the communicating between Agent part of instrumented App and the admin side
        /// </summary>
        public AdminOptions Admin { get; set; }
    }
}
