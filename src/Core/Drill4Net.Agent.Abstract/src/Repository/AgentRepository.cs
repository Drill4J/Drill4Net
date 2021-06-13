﻿using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Data repository for the profiling Agent
    /// </summary>
    /// <seealso cref="Drill4Net.Core.Repository.AbstractRepository{Drill4Net.Common.AgentOptions, Drill4Net.Core.Repository.BaseOptionsHelper{Drill4Net.Common.AgentOptions}}" />
    public class AgentRepository : AbstractRepository<AgentOptions, BaseOptionsHelper<AgentOptions>>
    {
        public AgentRepository(string cfgPath = null) : base(cfgPath)
        {
        }
    }
}
