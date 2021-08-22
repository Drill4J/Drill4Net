using System;
using Drill4Net.Common;
using Drill4Net.Agent.Kafka.Common;

namespace Drill4Net.Agent.Kafka.Service
{
    public class ServerSenderRepository : IMessageSenderRepository
    {
        public Guid Session { get; }

        public string Subsystem { get; }

        public string Target { get; set; }

        public MessageSenderOptions SenderOptions { get; set; }

        private readonly TargetInfo _targetInfo;

        /*******************************************************************************/

        public ServerSenderRepository(string target, TargetInfo targetInfo, MessageSenderOptions senderOptions)
        {
            _targetInfo = targetInfo ?? throw new ArgumentNullException(nameof(targetInfo));
            SenderOptions = senderOptions ?? throw new ArgumentNullException(nameof(senderOptions));
            Subsystem = CoreConstants.SUBSYSTEM_TRANSMITTER;
            Session = targetInfo.SessionUid;
            Target = target ?? targetInfo.Solution?.Name;
        }

        /*******************************************************************************/

        public byte[] GetTargetInfo()
        {
            return Serializer.ToArray<TargetInfo>(_targetInfo);
        }
    }
}
