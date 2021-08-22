namespace Drill4Net.Agent.Kafka.Transport
{
    public interface ITargetInfoReceiver : IMessageReceiver
    {
        event TargetReceivedInfoHandler TargetInfoReceived;
    }
}