using System;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    public interface IDataSender
    {
        bool IsError { get; }

        string LastError { get; }

        bool IsFatalError { get; }

        int SendTargetInfo(byte[] info);

        int SendProbe(string str);
    }
}