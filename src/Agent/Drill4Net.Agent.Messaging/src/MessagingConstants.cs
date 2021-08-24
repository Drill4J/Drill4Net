namespace Drill4Net.Agent.Messaging
{
    public static class MessagingConstants
    {
        #region Message headers
        public const string HEADER_SUBSYSTEM = "SUBSYSTEM";
        public const string HEADER_TARGET = "TARGET";
        public const string HEADER_REQUEST = "REQUEST";

        public const string HEADER_MESSAGE_TYPE = "TYPE";
        public const string HEADER_MESSAGE_PACKET = "PACKET";
        public const string HEADER_MESSAGE_PACKETS = "PACKETS";
        public const string HEADER_MESSAGE_COMPRESSED_SIZE = "COMPRESSED_SIZ";
        public const string HEADER_MESSAGE_DECOMPRESSED_SIZE = "DECOMPRESSED_SIZE";
        #endregion
        #region Message type
        public const string MESSAGE_TYPE_PING = "Ping";
        public const string MESSAGE_TYPE_PROBE = "Probe";
        public const string MESSAGE_TYPE_TARGET_INFO = "Target";
        #endregion
        #region Topics
        public const string TOPIC_PING = "ping";
        public const string TOPIC_TARGET_INFO = "target-info";
        #endregion
        #region Ping data
        public const string PING_SUBSYSTEM = "subsystem";
        public const string PING_TIME = "time";
        public const string PING_TARGET_SESSION = "target_session";
        #endregion

        public const int MaxMessageSize = 1000000;
    }
}
