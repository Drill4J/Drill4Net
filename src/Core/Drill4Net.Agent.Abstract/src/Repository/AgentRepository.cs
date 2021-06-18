using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Data repository for the profiling Agent
    /// </summary>
    public class AgentRepository : AbstractRepository<AgentOptions, BaseOptionsHelper<AgentOptions>>
    {
        public AgentRepository(string cfgPath = null) : base(cfgPath, CoreConstants.SUBSYSTEM_AGENT)
        {
        }
    }
}
