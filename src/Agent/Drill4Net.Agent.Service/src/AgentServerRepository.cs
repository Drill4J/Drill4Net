using Drill4Net.Agent.Messaging.Transport;

namespace Drill4Net.Agent.Service
{
    public class AgentServerRepository : MessageReceiverRepository<AgentServerOptions>
    {
        public AgentServerRepository(string subsystem, string cfgPath = null) : base(subsystem, cfgPath)
        {
        }

        public AgentServerRepository(string subsystem, AgentServerOptions opts) : base(subsystem, opts)
        {
        }
    }
}
