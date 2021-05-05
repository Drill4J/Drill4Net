namespace Drill4Net.Agent.Abstract.Transfer
{
    public abstract class OutgoingMessage : AbstractMessage
    {
        protected OutgoingMessage(string type) : base(type)
        {
        }
    }
}