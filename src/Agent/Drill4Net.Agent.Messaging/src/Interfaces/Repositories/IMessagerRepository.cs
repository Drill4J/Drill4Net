using System;

namespace Drill4Net.Agent.Messaging
{
    public interface IMessagerRepository
    {
        MessagerOptions MessagerOptions { get; }
        Guid TargetSession { get; }
        string TargetName { get; }
        string Subsystem { get; }
    }
}
