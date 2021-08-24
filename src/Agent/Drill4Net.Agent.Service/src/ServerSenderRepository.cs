using System;
using Drill4Net.Common;
using Drill4Net.Agent.Messaging;

namespace Drill4Net.Agent.Service
{
    public class ServerSenderRepository : ITargetSenderRepository
    {
        public Guid TargetSession { get; }

        public string Subsystem { get; }

        public string Target { get; set; }

        public MessageSenderOptions SenderOptions { get; set; }

        private readonly TargetInfo _targetInfo;

        /*******************************************************************************/

        public ServerSenderRepository(string target, TargetInfo targetInfo, MessageSenderOptions senderOptions)
        {
            Subsystem = CoreConstants.SUBSYSTEM_PROBE_SERVER;
            _targetInfo = targetInfo ?? throw new ArgumentNullException(nameof(targetInfo));
            SenderOptions = senderOptions ?? throw new ArgumentNullException(nameof(senderOptions));
            TargetSession = targetInfo.SessionUid;
            Target = target ?? targetInfo.Solution?.Name;
        }

        /*******************************************************************************/

        public byte[] GetTargetInfo()
        {
            return Serializer.ToArray<TargetInfo>(_targetInfo);
        }
    }
}
