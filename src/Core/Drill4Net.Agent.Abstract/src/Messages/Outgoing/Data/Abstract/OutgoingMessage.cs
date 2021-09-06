namespace Drill4Net.Agent.Abstract.Transfer
{
    public abstract record OutgoingMessage: AbstractMessage
    {
        protected OutgoingMessage(string type): base(type)
        {
        }
    }
}