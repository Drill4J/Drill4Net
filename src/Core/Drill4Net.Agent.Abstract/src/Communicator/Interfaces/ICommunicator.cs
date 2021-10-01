namespace Drill4Net.Agent.Abstract
{
    public interface ICommunicator
    {
        IAgentReceiver Receiver { get; }
        AbstractCoverageSender Sender { get; }

        void Connect();
    }
}
