namespace Drill4Net.Agent.Abstract
{
    public interface ICommunicator
    {
        IReceiver Receiver { get; }
        AbstractSender Sender { get; }
    }
}
