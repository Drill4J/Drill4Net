using System;
using Drill4Net.Agent.Kafka.Common;
using Drill4Net.Common;

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

        public ServerSenderRepository(string target, string subsystem, TargetInfo targetInfo,
            MessageSenderOptions senderOptions)
        {
            _targetInfo = targetInfo ?? throw new ArgumentNullException(nameof(targetInfo));
            SenderOptions = senderOptions ?? throw new ArgumentNullException(nameof(senderOptions));
            Session = targetInfo.SessionUid;
            Target = target;
            Subsystem = subsystem;
        }

        /*******************************************************************************/

        public byte[] GetTargetInfo()
        {
            var bytes = Serializer.ToArray<TargetInfo>(_targetInfo);
            return bytes;
        }
    }
}
