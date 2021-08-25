namespace Drill4Net.Agent.Messaging.Transport
{
    public delegate void TargetReceivedInfoHandler(TargetInfo target);

    /***********************************************************************/

    public interface ITargetInfoReceiver : IMessageReceiver
    {
        event TargetReceivedInfoHandler TargetInfoReceived;
    }
}