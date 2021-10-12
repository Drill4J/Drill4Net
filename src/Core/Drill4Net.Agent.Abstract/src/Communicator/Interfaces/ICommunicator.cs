namespace Drill4Net.Agent.Abstract
{
    public interface ICommunicator
    {
        IAgentReceiver Receiver { get; }
        AbstractCoveragerSender Sender { get; }

        void Connect();
    }
}
