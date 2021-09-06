namespace Drill4Net.Agent.Abstract.Transfer
{
    public record CancelAllAgentSessions : IncomingMessage
    {
        public CancelAllAgentSessions() : base(AgentConstants.MESSAGE_IN_CANCEL_ALL)
        {
        }
    }
}