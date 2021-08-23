namespace Drill4Net.Agent.Messaging.Transport
{
    public interface ITargetInfoReceiver : IMessageReceiver
    {
        event TargetReceivedInfoHandler TargetInfoReceived;
    }
}