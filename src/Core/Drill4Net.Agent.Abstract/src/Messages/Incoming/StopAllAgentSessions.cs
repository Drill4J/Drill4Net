namespace Drill4Net.Agent.Abstract.Transfer
{
    public class StopAllAgentSessions : IncomingMessage
    {
        public StopAllAgentSessions() : base(AgentConstants.MESSAGE_IN_STOP_ALL)
        {
        }
    }
}