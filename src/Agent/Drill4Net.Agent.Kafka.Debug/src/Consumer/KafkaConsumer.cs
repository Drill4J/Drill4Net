using System;
using System.Linq;
using System.Threading;
using Confluent.Kafka;
using Drill4Net.Common;

namespace Drill4Net.Agent.Kafka.Debug
{
    //https://github.com/patsevanton/docker-compose-kafka-zk-kafdrop-cmak/blob/main/docker-compose.yml

    public delegate void ReceivedMessageHandler(string message);
    public delegate void ErrorOccuredHandler(bool isFatal, bool isLocal, string message);

    public class KafkaConsumer : IProbeReceiver
    {
        public event ReceivedMessageHandler MessageReceived;
        public event ErrorOccuredHandler ErrorOccured;

        private readonly ConsumerConfig _cfg;
        private readonly AbstractRepository<ConverterOptions> _rep;

        /****************************************************************************************/

        public KafkaConsumer(AbstractRepository<ConverterOptions> rep)
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
                // automatically, so in this example, consumption will only start from the
                // earliest message in the topic 'my-topic' the first time you run the program.
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
        }

        /****************************************************************************************/

        public void Start()
        {
            var opts = _rep.Options;

            using var c = new ConsumerBuilder<Ignore, string>(_cfg).Build();
            c.Subscribe(opts.Topic);

            CancellationTokenSource cts = new();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true; // prevent the process from terminating.
                cts.Cancel();
            };

            try
            {
                while (true)
                {
                    try
                    {
                        var cr = c.Consume(cts.Token);
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
    }
}
