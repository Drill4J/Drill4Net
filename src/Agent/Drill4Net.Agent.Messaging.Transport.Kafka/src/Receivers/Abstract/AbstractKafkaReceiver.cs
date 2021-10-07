﻿using System;
using Confluent.Kafka;
using Drill4Net.BanderLog;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    public abstract class AbstractKafkaReceiver<T> : IMessageReceiver
        where T : MessageReceiverOptions, new()
    {
        public event ErrorOccuredDelegate ErrorOccured;

        public bool IsStarted { get; protected set; }

        protected readonly ConsumerConfig _cfg;
        protected readonly AbstractRepository<T> _rep;
        private readonly Logger _logger;

        private int _unknownTopicCounter;

        /*****************************************************************************************/

        protected AbstractKafkaReceiver(AbstractRepository<T> rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _logger = new TypedLogger<AbstractKafkaReceiver<T>>(rep.Subsystem);
            var opts = _rep.Options;

            _cfg = new ConsumerConfig
            {
                GroupId = opts.GroupId,
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