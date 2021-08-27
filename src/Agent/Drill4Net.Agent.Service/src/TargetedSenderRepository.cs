using System;
using Drill4Net.Common;
using Drill4Net.Agent.Messaging;

namespace Drill4Net.Agent.Service
{
    public class TargetedSenderRepository : ITargetSenderRepository
    {
        public Guid TargetSession { get; }

        public string Subsystem { get; }

        public string TargetName { get; set; }

        public MessageSenderOptions SenderOptions { get; set; }

        private readonly TargetInfo _targetInfo;

        /*******************************************************************************/

        public TargetedSenderRepository(TargetInfo targetInfo, MessageSenderOptions senderOptions)
        {
            Subsystem = CoreConstants.SUBSYSTEM_AGENT_SERVER;
            _targetInfo = targetInfo ?? throw new ArgumentNullException(nameof(targetInfo));
            SenderOptions = senderOptions ?? throw new ArgumentNullException(nameof(senderOptions));
            TargetName = targetInfo.TargetName ?? targetInfo.Solution?.Name;
            TargetSession = targetInfo.SessionUid;
        }

        /*******************************************************************************/

        public byte[] GetTargetInfo()
        {
            return Serializer.ToArray<TargetInfo>(_targetInfo);
        }
    }
}
