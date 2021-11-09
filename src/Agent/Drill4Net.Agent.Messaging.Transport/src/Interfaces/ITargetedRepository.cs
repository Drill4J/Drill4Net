using System;

namespace Drill4Net.Agent.Messaging.Transport
{
    public interface ITargetedRepository
    {
        string Subsystem { get; }
        string ConfigPath { get; }
        string TargetName { get; }
        Guid TargetSession { get; }
    }
}