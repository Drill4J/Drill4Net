namespace Drill4Net.Agent.Messaging
{
    public interface IDataSender
    {
        bool IsError { get; }

        string LastError { get; }

        bool IsFatalError { get; }

        int SendTargetInfo(byte[] info, string topic = MessagingConstants.TOPIC_TARGET_INFO);

        /// <summary>
        /// Sends the specified probe to the middleware.
        /// </summary>
        /// <param name="data">The cross-point data.</param>
        /// <param name="ctx">The context of data (user, process, worker, etc)</param>
        int SendProbe(string data, string ctx);
    }
}