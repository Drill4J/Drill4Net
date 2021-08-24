namespace Drill4Net.Agent.Messaging
{
    public interface IProbeSender : IDataSender
    {
        /// <summary>
        /// Sends the specified probe to the middleware.
        /// </summary>
        /// <param name="data">The cross-point data.</param>
        /// <param name="ctx">The context of data (user, process, worker, etc)</param>
        int SendProbe(string data, string ctx);
    }
}
