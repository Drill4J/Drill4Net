namespace Drill4Net.Agent.Messaging
{
    public interface ITargetedInfoSenderRepository : IMessagerRepository
    {
        string Directory { get; }
        byte[] GetTargetInfo();
    }
}