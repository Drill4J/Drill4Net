using System;

namespace Drill4Net.Agent.Messaging.Transport
{
    public class TargetedReceiverRepository : MessageReceiverRepository<MessageReceiverOptions>
    {
        public Guid TargetSession { get; private set; }
        public BaseMessageOptions SenderOptions { get; set; }

        /***************************************************************************************************/

        public TargetedReceiverRepository(): base(null) { }

        public TargetedReceiverRepository(string subsystem, string targetSession, string cfgPath = null): base(subsystem, cfgPath)
        {
            Init(targetSession);
        }

        public TargetedReceiverRepository(string subsystem, string targetSession, MessageReceiverOptions opts): base(subsystem, opts)
        {
            Init(targetSession);
        }

        /***************************************************************************************************/

        private void Init(string targetSession)
        {
            TargetSession = Guid.Parse(targetSession);
            PrepareLogger();
        }
    }
}
