namespace Drill4Net.Agent.Kafka.Common
{
    public static class KafkaConstants
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
        public const string MESSAGE_TYPE_PROBE = "Probe";
        public const string MESSAGE_TYPE_TARGET_INFO = "Target";
        #endregion

        public const string TOPIC_TARGET_INFO = "target-info";

        public const int MaxMessageSize = 1000000;
    }
}
