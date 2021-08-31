using Drill4Net.Agent.Messaging.Transport;

namespace Drill4Net.Agent.Worker
{
    public class AgentWorkerRepository : MessageReceiverRepository<MessageReceiverOptions>
    {
        public string TargetSession { get; private set; }

        /***************************************************************************************************/

        public AgentWorkerRepository(string subsystem, string targetSession, string cfgPath = null) : base(subsystem, cfgPath)
        {
            Init(targetSession);
        }

        public AgentWorkerRepository(string subsystem, string targetSession, MessageReceiverOptions opts) : base(subsystem, opts)
        {
            Init(targetSession);
        }

        /***************************************************************************************************/

        private void Init(string targetSession)
        {
            TargetSession = targetSession;
            PrepareLogger();
        }
    }
}
