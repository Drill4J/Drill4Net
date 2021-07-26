using System;
using System.Collections.Generic;
using Confluent.Kafka;
using Drill4Net.Agent.Kafka.Common;
using Drill4Net.Common;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    public class KafkaProducer : IProbeSender
    {
        public bool IsError { get; private set; }

        public string LastError { get; private set; }

        public bool IsFatalError { get; private set; }

        private readonly int _messageMaxSize;
        private readonly Headers _headers;
        private readonly List<string> _topics;
        private readonly ProducerConfig _cfg;
        private readonly IProducer<Null, string> _producer;
        private readonly TransmitterRepository _rep;

        /***************************************************************************************/

        public KafkaProducer(TransmitterRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            var transOpts = _rep.TransmitterOptions;

            _cfg = new ProducerConfig
            {
                BootstrapServers = string.Join(",", transOpts.Servers),
            };

            //https://stackoverflow.com/questions/21020347/how-can-i-send-large-messages-with-kafka-over-15mb
            _messageMaxSize = _cfg.MessageMaxBytes ?? KafkaConstants.MaxMessageSize;

            _headers = new Headers
            {
                new Header(KafkaConstants.HEADER_MESSAGE_TYPE, _rep.SerializeToByte(KafkaConstants.MESSAGE_TYPE_PROBE)),
                new Header(KafkaConstants.HEADER_SUBSYSTEM, _rep.SerializeToByte(_rep.Subsystem)),
                new Header(KafkaConstants.HEADER_TARGET, _rep.SerializeToByte(_rep.Target)),
            };
            _topics = transOpts.Topics;
            _producer = new ProducerBuilder<Null, string>(_cfg).Build();
        }

        ~KafkaProducer()
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }

        /***************************************************************************************/

        public int Send(string str)
        {
            foreach (var topic in _topics)
            {
                var mess = new Message<Null, string> { Value = str, Headers = _headers };
                _producer.Produce(topic, mess, Handle);
            }

            if (!IsError) //if there is no connection, it will come here without an error :(
                return 0;
            return IsFatalError ? -2 : -1;
        }

        private void Handle(DeliveryReport<Null, string> report)
        {
            var err = report.Error;
            IsError = err.IsError;
            IsFatalError = err.IsFatal;
            LastError = err.Reason;
        }
    }
}
