namespace Drill4Net.Agent.Abstract.Messages
{
    /// <summary>
    /// Represents a message from Drill admin.
    /// </summary>
    public class ConnectorQueueItem
    {
        /// <summary>
        /// Type of the message.
        /// </summary>
        public string Destination { get; set; }
        
        /// <summary>
        /// The message payload.
        /// </summary>
        public string Message { get; set; }
    }
}