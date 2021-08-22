namespace Drill4Net.Agent.Kafka.Common
{
    public interface IDataSender
    {
        bool IsError { get; }

        string LastError { get; }

        bool IsFatalError { get; }

        int SendTargetInfo(byte[] info);

        /// <summary>
        /// Sends the specified probe to the middleware.
        /// </summary>
        /// <param name="data">The cross-point data.</param>
        /// <param name="ctx">The context of data (user, process, worker, etc)</param>
        int SendProbe(string data, string ctx);
    }
}