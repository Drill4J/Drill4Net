namespace Drill4Net.Agent.Messaging.Transport
{
    public delegate void CommandReceivedHandler(Command command);

    /*************************************************************/

    public interface ICommandReceiver : IMessageReceiver
    {
        event CommandReceivedHandler CommandReceived;
    }
}
