using System;

namespace Drill4Net.Agent.Messaging
{
    public interface IMessageSenderRepository
    {
        MessageSenderOptions SenderOptions { get; set; }
        Guid Session { get; }
        string Target { get; set; }
        string Subsystem { get; }

        byte[] GetTargetInfo();
    }
}