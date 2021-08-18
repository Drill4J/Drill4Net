﻿using System;
using Drill4Net.Agent.Abstract;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Agent.Kafka.Common
{
    /// <summary>
    /// Metadata about a Target system, its entities' Tree, some injecting options, etc
    /// </summary>
    [Serializable]
    public class TargetInfo
    {
        public Guid Uid { get; set; }

        public AgentOptions Options { get; set; }

        public InjectedSolution Solution { get; set; }
    }
}
