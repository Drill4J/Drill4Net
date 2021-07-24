namespace Drill4Net.Agent.Kafka.Transmitter
{
    public interface IProbeSender
    {
        bool IsError { get; }

        string LastError { get; }

        bool IsFatalError { get; }

        int Send(string str);
    }
}