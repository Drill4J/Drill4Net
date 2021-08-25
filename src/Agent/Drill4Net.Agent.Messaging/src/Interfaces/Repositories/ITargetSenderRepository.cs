namespace Drill4Net.Agent.Messaging
{
    public interface ITargetSenderRepository : IMessageSenderRepository
    {
        byte[] GetTargetInfo();
    }
}