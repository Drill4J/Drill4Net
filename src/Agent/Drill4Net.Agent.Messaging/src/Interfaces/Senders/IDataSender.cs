using System;

namespace Drill4Net.Agent.Messaging
{
    public interface IDataSender : IDisposable
    {
        bool IsError { get; }

        string LastError { get; }

        bool IsFatalError { get; }
    }
}