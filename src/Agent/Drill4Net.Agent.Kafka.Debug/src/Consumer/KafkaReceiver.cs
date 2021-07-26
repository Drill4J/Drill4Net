using System;
using System.Linq;
using System.Threading;
using Confluent.Kafka;
using Drill4Net.Agent.Kafka.Common;
using Drill4Net.Common;

namespace Drill4Net.Agent.Kafka.Debug
{
    //https://github.com/patsevanton/docker-compose-kafka-zk-kafdrop-cmak/blob/main/docker-compose.yml

    public delegate void ReceivedMessageHandler(string message);
    public delegate void ErrorOccuredHandler(bool isFatal, bool isLocal, string message);

    public class KafkaReceiver : IProbeReceiver
    {
        public event ReceivedMessageHandler MessageReceived;
        public event ErrorOccuredHandler ErrorOccured;

        private CancellationTokenSource _cts;
        private readonly ConsumerConfig _cfg;
        private readonly AbstractRepository<ConverterOptions> _rep;

        /****************************************************************************************/

        public KafkaReceiver(AbstractRepository<ConverterOptions> rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
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
                MessageMaxBytes = KafkaConstants.MaxMessageSize,
            };
        }

        /****************************************************************************************/

        public void Start()
        {
            var opts = _rep.Options;
            _cts = new();

            using var c = new ConsumerBuilder<Ignore, string>(_cfg).Build();
            c.Subscribe(opts.Topics);

            try
            {
                while (true)
                {
                    try
                    {
                        var cr = c.Consume(_cts.Token);
                        var val = cr.Message.Value;
                        MessageReceived?.Invoke(val);
                    }
                    catch (ConsumeException e)
                    {
                        var err = e.Error;
                        ErrorOccured?.Invoke(err.IsFatal, err.IsLocalError, err.Reason);
                    }
                }
            }
            catch (OperationCanceledException opex)
            {
                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                c.Close();

                ErrorOccured?.Invoke(true, false, opex.Message);
            }
        }

        public void Stop()
        {
            _cts.Cancel();
        }
    }
}
