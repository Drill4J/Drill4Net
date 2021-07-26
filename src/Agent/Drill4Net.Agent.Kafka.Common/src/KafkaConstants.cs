namespace Drill4Net.Agent.Kafka.Common
{
    public static class KafkaConstants
    {
        #region Message headers
        public const string HEADER_MESSAGE_TYPE = "TYPE";
        public const string HEADER_SUBSYSTEM = "SUBSYSTEM";
        public const string HEADER_TARGET = "TARGET";
        #endregion
        #region Message type
        public const string MESSAGE_TYPE_PROBE = "Probe";
        public const string MESSAGE_TYPE_TARGET_INFO = "Target";
        #endregion

        public const int MaxMessageSize = 1000000;
    }
}
