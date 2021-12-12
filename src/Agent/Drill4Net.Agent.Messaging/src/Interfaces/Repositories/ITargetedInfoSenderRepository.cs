namespace Drill4Net.Agent.Messaging
{
    public interface ITargetedInfoSenderRepository : IMessagerRepository
    {
        byte[] GetTargetInfo();
    }
}