using System;
using Drill4Net.Common;
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.Messaging.Transport;

namespace Drill4Net.Agent.Service
{
    public class TargetedInfoSenderRepository : TargetedSenderRepository, ITargetSenderRepository
    {
        private readonly TargetInfo _targetInfo;

        /*******************************************************************************/

        public TargetedInfoSenderRepository(TargetInfo targetInfo, BaseMessageOptions senderOptions):
            base(CoreConstants.SUBSYSTEM_AGENT_SERVER, targetInfo.SessionUid, targetInfo.TargetName ?? targetInfo.Solution?.Name, senderOptions)
        {
            _targetInfo = targetInfo;
        }

        /*******************************************************************************/

        public byte[] GetTargetInfo()
        {
            return Serializer.ToArray<TargetInfo>(_targetInfo);
        }
    }
}
