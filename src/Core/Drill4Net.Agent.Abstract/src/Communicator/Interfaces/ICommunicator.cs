namespace Drill4Net.Agent.Abstract
{
    public interface ICommunicator
    {
        IAgentReceiver Receiver { get; }
        AbstractSender Sender { get; }

        void Connect();
    }
}
