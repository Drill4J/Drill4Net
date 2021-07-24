using System;
using Confluent.Kafka;
using Drill4Net.Common;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    public class KafkaProducer : IProbeSender
    {
        public bool IsError { get; private set; }

        public string LastError { get; private set; }

        public bool IsFatalError { get; private set; }

        private readonly string _topic;
        private readonly ProducerConfig _cfg;
        private readonly IProducer<Null, string> _producer;
        private readonly AbstractRepository<TransmitterOptions> _rep;

        /****************************************************************************/

        public KafkaProducer(AbstractRepository<TransmitterOptions> rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));

            var servers = string.Join(",", _rep.Options.Servers);
            _cfg = new ProducerConfig { BootstrapServers = servers };
            _topic = _rep.Options.Topic;
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
            _producer.Produce(_topic, new Message<Null, string> { Value = str }, Handle);

            if (!IsError) //если нет коннекта сюда придёт без ошибки
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
