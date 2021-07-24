namespace Drill4Net.Agent.Kafka.Transmitter
{
    public interface IProbeSender
    {
        int Send(string str);
    }
}