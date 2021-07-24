using System;
using System.Linq;
using System.Threading;
using Confluent.Kafka;

namespace Drill4Net.Agent.Kafka.Debug
{
    public class KafkaConsumer
    {
        private readonly ConsumerConfig _cfg;
        private readonly KafkaConsumerRepository _rep;

        /*******************************************************************/

        public KafkaConsumer(KafkaConsumerRepository rep)
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

            Consume();
        }

        /*******************************************************************/

        private void Consume()
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
                        Console.WriteLine($"Consumed message '{cr.Value}' at: '{cr.TopicPartitionOffset}'.");
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Error occured: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                c.Close();
            }
        }
    }
}
