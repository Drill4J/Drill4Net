using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Data repository for the profiling Agent
    /// </summary>
    public class AgentRepository : TreeRepository<AgentOptions, BaseOptionsHelper<AgentOptions>>
    {
        public AgentRepository(string cfgPath = null) : base(CoreConstants.SUBSYSTEM_AGENT, cfgPath)
        {
        }

        public AgentRepository(AgentOptions opts) : base(CoreConstants.SUBSYSTEM_AGENT, opts)
        {
        }
    }
}
