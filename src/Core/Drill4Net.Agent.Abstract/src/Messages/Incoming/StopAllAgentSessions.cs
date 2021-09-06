namespace Drill4Net.Agent.Abstract.Transfer
{
    public record StopAllAgentSessions : IncomingMessage
    {
        public StopAllAgentSessions() : base(AgentConstants.MESSAGE_IN_STOP_ALL)
        {
        }
    }
}