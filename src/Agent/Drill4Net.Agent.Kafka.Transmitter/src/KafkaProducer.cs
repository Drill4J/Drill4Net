using System;
using System.Collections.Generic;
using System.Text;
using Confluent.Kafka;
using Drill4Net.Common;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    public class KafkaProducer : IProbeSender
    {
        public bool IsError { get; private set; }

        public string LastError { get; private set; }

        public bool IsFatalError { get; private set; }

        private readonly Headers _headers;
        private readonly List<string> _topics;
        private readonly ProducerConfig _cfg;
        private readonly IProducer<Null, string> _producer;
        private readonly TransmitterRepository _rep;

        /****************************************************************************/

        public KafkaProducer(TransmitterRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));

            var servers = string.Join(",", _rep.Options.Servers);
            _cfg = new ProducerConfig { BootstrapServers = servers };

            var opts = _rep.Options;
            _topics = opts.Topics;

            _headers = new Headers
            {
                new Header(CoreConstants.HEADER_SUBSYSTEM, _rep.SerializeToByte(_rep.Subsystem)),
                new Header(CoreConstants.HEADER_TARGET, _rep.SerializeToByte(_rep.Target)),
            };

            _producer = new ProducerBuilder<Null, string>(_cfg).Build();
        }

        ~KafkaProducer()
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }

        /****************************************************************************/

        public int Send(string str)
        {
            //var headers = new
            foreach (var topic in _topics)
            {
                var mess = new Message<Null, string> { Value = str, Headers = _headers };
                _producer.Produce(topic, mess, Handle);
            }

            if (!IsError) //если нет коннекта, сюда придёт без ошибки
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
