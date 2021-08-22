using System;

namespace Drill4Net.Agent.Kafka.Common
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