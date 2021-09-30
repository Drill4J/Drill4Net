namespace Drill4Net.Agent.Messaging.Transport
{
    public class AgentWorkerRepository : MessageReceiverRepository<MessageReceiverOptions>
    {
        public string TargetSession { get; private set; }

        /***************************************************************************************************/

        public AgentWorkerRepository():base(null) { }

        public AgentWorkerRepository(string subsystem, string targetSession, string cfgPath = null): base(subsystem, cfgPath)
        {
            Init(targetSession);
        }

        public AgentWorkerRepository(string subsystem, string targetSession, MessageReceiverOptions opts): base(subsystem, opts)
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
