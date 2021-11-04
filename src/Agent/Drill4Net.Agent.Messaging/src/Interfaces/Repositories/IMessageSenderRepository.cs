using System;

namespace Drill4Net.Agent.Messaging
{
    public interface IMessageSenderRepository
    {
        BaseMessageOptions SenderOptions { get; set; }
        Guid TargetSession { get; }
        string TargetName { get; set; }
        string Subsystem { get; }
    }
}
