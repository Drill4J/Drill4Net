using Drill4Net.Profiling.Tree;
using System;

namespace Drill4Net.Agent.Kafka.Common
{
    /// <summary>
    /// Metadata about a Target system, its entities' Tree, some injecting options, etc
    /// </summary>
    [Serializable]
    public class TargetInfo
    {
        public InjectedSolution Solution { get; set; }
    }
}
