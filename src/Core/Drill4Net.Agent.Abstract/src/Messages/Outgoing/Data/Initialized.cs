namespace Drill4Net.Agent.Abstract.Transfer
{
    public class Initialized : AbstractMessage
    {
        public string Msg { get; set; }

        public Initialized() : base(AgentConstants.MESSAGE_OUT_INITIALIZED)
        {
        }
    }
}