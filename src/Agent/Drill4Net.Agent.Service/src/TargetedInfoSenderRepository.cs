using Drill4Net.Common;
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.Messaging.Transport;

namespace Drill4Net.Agent.Service
{
    public class TargetedInfoSenderRepository : TargetedSenderRepository, ITargetedInfoSenderRepository
    {
        public string Directory { get; }

        private readonly TargetInfo _targetInfo;

        /*******************************************************************************/

        public TargetedInfoSenderRepository(TargetInfo targetInfo, MessagerOptions senderOptions):
            base(CoreConstants.SUBSYSTEM_AGENT_SERVER, targetInfo.Session, targetInfo.Name ?? targetInfo.Data?.Name,
                targetInfo.Version ?? targetInfo.Data?.ProductVersion, senderOptions)
        {
            Directory = FileUtils.EntryDir;
            _targetInfo = targetInfo;
        }

        /*******************************************************************************/

        public byte[] GetTargetInfo()
        {
            return Serializer.ToArray<TargetInfo>(_targetInfo);
        }
    }
}
