namespace Drill4Net.Agent.Messaging
{
    public interface ITargetInfoSender : IDataSender
    {
        int SendTargetInfo(byte[] info, string topic = null);
    }
}
