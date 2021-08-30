using Drill4Net.Agent.Messaging.Transport;

namespace Drill4Net.Agent.Worker
{
    public class AgentWorkerRepository : MessageReceiverRepository<MessageReceiverOptions>
    {
        public AgentWorkerRepository(string subsystem, string cfgPath = null) : base(subsystem, cfgPath)
        {
            PrepareLogger();
        }

        public AgentWorkerRepository(string subsystem, MessageReceiverOptions opts) : base(subsystem, opts)
        {
            PrepareLogger();
        }
    }
}
