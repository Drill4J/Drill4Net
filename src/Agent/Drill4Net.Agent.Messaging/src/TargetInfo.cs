using System;
using Drill4Net.Agent.Abstract;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Agent.Messaging
{
    /// <summary>
    /// Metadata about a Target system, its entities' Tree, Agent options, etc
    /// </summary>
    [Serializable]
    public class TargetInfo
    {
        /// <summary>
        /// Gets or sets the uid of the current info object.
        /// </summary>
        /// <value>
        /// The uid of the current object.
        /// </value>
        public Guid Uid { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public string Version { get; set; }

        public string EntryDir { get; set; }

        public string EntryAssembly { get; set; }

        /// <summary>
        /// Gets or sets the session uid.
        /// </summary>
        /// <value>
        /// The session uid.
        /// </value>
        public Guid Session { get; set; }

        /// <summary>
        /// Gets or sets the Agent options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public AgentOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the Tree of injected entities of Target.
        /// </summary>
        /// <value>
        /// The Tree of injected entities of Target.
        /// </value>
        public InjectedSolution Tree { get; set; }

        /*******************************************************************/

        public override string ToString()
        {
            return $"{Session} -> {Name} {Version}";
        }
    }
}
