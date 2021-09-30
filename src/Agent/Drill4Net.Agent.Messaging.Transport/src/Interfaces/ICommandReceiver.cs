namespace Drill4Net.Agent.Messaging.Transport
{
    public delegate void CommandReceivedHandler(Command probe);

    /*************************************************************/

    public interface ICommandReceiver : IMessageReceiver
    {
        event CommandReceivedHandler CommandReceived;
    }
}
