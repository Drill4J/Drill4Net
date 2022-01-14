using System;
using Confluent.Kafka;
using Drill4Net.BanderLog;
using Drill4Net.Repository;

namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    public abstract class AbstractKafkaReceiver<TOpts> : IMessageReceiver
        where TOpts : MessagerOptions, new()
    {
        public event ErrorOccuredDelegate ErrorOccured;

        public bool IsStarted { get; protected set; }

        protected readonly ConsumerConfig _cfg;
        protected readonly AbstractRepository<TOpts> _rep;
        private readonly Logger _logger;

        private int _unknownTopicCounter;

        /*****************************************************************************************/

        protected AbstractKafkaReceiver(AbstractRepository<TOpts> rep)
        {
            #region Init/checks
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _logger = new TypedLogger<AbstractKafkaReceiver<TOpts>>(rep.Subsystem);
            var opts = _rep.Options;
            if(opts == null)
                throw new Exception("Options are empty");
            if (opts.Receiver == null)
                throw new Exception("Receiver is empty");
            if (opts.Servers == null || opts.Servers.Count == 0)
                throw new Exception("Servers are empty");
            #endregion

            _cfg = new ConsumerConfig
            {
                GroupId = opts.Receiver.GroupId,
                BootstrapServers = string.Join(",", opts.Servers),

                // Note: The AutoOffsetReset property determines the start offset in the event
                // there are not yet any committed offsets for the consumer group for the
                // topic/partitions of interest. By default, offsets are committed
                // automatically, so in this case, consumption will only start from the
                // earliest message in the topic the first time you run the program.
                AutoOffsetReset = AutoOffsetReset.Earliest,

                AllowAutoCreateTopics = true,
                EnableAutoCommit = true,
                EnableAutoOffsetStore = true,
                MessageMaxBytes = MessagingConstants.MaxMessageSize,
            };
        }

        /*****************************************************************************************/

        public abstract void Start();

        public abstract void Stop();

        protected void ProcessConsumeExcepton(ConsumeException e)
        {
            var err = e.Error;
            var code = err.Code;
            var mess = $"({code}) {err.Reason}";

            //Server can sent the info a little later than this method starts
            if (code == ErrorCode.UnknownTopicOrPart)
                _unknownTopicCounter++;
            if (code != ErrorCode.UnknownTopicOrPart || _unknownTopicCounter > 5)
                ErrorOccuredHandler(this, err.IsFatal, err.IsLocalError, mess);
        }

        protected void ErrorOccuredHandler(IMessageReceiver source, bool isFatal, bool isLocal, string message)
        {
            ErrorOccured?.Invoke(source, isFatal, isLocal, message);
        }
    }
}
