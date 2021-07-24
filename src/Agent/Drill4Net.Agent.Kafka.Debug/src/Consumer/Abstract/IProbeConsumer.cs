namespace Drill4Net.Agent.Kafka.Debug
{
    public interface IProbeConsumer
    {
        event ErrorOccuredHandler ErrorOccured;
        event ReceivedMessageHandler MessageReceived;

        void Consume();
    }
}