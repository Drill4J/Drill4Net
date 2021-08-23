﻿using System;
using Confluent.Kafka;
using Drill4Net.Common;
using Drill4Net.Agent.Kafka.Common;

namespace Drill4Net.Agent.Kafka.Transport
{
    public abstract class AbstractKafkaReceiver : IMessageReceiver
    {
        public event ErrorOccuredDelegate ErrorOccured;

        protected readonly ConsumerConfig _cfg;
        protected readonly AbstractRepository<MessageReceiverOptions> _rep;

        protected readonly string _logPrefix;

        /*************************************************************************************************/

        public AbstractKafkaReceiver(AbstractRepository<MessageReceiverOptions> rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _logPrefix = TransportUtils.GetLogPrefix(rep.Subsystem, GetType());
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

                EnableAutoCommit = true,
                EnableAutoOffsetStore = true,
                MessageMaxBytes = MessagingConstants.MaxMessageSize,
            };
        }

        /*************************************************************************************************/

        public abstract void Start();

        public abstract void Stop();

        protected void ErrorOccuredHandler(bool isFatal, bool isLocal, string message)
        {
            ErrorOccured?.Invoke(isFatal, isLocal, message);
        }
    }
}
