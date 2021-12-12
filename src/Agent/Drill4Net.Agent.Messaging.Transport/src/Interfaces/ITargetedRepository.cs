using System;

namespace Drill4Net.Agent.Messaging.Transport
{
    public interface ITargetedRepository
    {
        string Subsystem { get; }
        string ConfigPath { get; }
        string TargetName { get; }
        string TargetVersion { get; }
        Guid TargetSession { get; }
    }
}