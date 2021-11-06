using System;

namespace Drill4Net.Agent.Messaging.Transport
{
    public class TargetedSenderRepository : IMessageSenderRepository
    {
        public string Subsystem { get; }

        public Guid TargetSession { get; }

        public string TargetName { get; set; }

        public BaseMessageOptions SenderOptions { get; set; }

        /***************************************************************************************/

        public TargetedSenderRepository(string subsystem, Guid targetSession, string targetName, BaseMessageOptions senderOptions)
        {
            Subsystem = subsystem ?? throw new ArgumentNullException(nameof(subsystem));
            SenderOptions = senderOptions ?? throw new ArgumentNullException(nameof(senderOptions));
            TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
            TargetSession = targetSession;
        }
    }
}
