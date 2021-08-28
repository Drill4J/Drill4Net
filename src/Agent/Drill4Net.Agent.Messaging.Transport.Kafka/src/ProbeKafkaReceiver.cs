using System;
using System.Linq;
using System.Threading;
using Confluent.Kafka;
using Drill4Net.Core.Repository;
using Drill4Net.Agent.Messaging.Kafka;

namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    public class ProbeKafkaReceiver : AbstractKafkaReceiver<MessageReceiverOptions>, IProbeReceiver
    {
        public event ProbeReceivedHandler ProbeReceived;

        private CancellationTokenSource _probesCts;

        /****************************************************************************************/

        public ProbeKafkaReceiver(AbstractRepository<MessageReceiverOptions> rep) : base(rep)
        {
        }

        /****************************************************************************************/

        public override void Start()
        {
            Stop();
            RetrieveProbes();
        }

        public override void Stop()
        {
            _probesCts?.Cancel();
        }

        private void RetrieveProbes()
        {
            Console.WriteLine($"{_logPrefix}Start retrieving probes...");

            _probesCts = new();
            var opts = _rep.Options;
            var probeTopics = MessagingUtils.FilterProbeTopics(opts.Topics);
            Console.WriteLine($"{_logPrefix}Probe topics: {string.Join(",", probeTopics)}");

            using var c = new ConsumerBuilder<Ignore, Probe>(_cfg)
                .SetValueDeserializer(new ProbeDeserializer())
                .Build();
            c.Subscribe(probeTopics);

            try
            {
                while (true)
                {
                    try
                    {
                        var cr = c.Consume(_probesCts.Token);
                        var probe = cr.Message.Value;
                        ProbeReceived?.Invoke(probe);
                    }
                    catch (ConsumeException e)
                    {
                        var err = e.Error;
                        ErrorOccuredHandler(err.IsFatal, err.IsLocalError, err.Reason);
                    }
                }
            }
            catch (OperationCanceledException opex)
            {
                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                c.Close();

                ErrorOccuredHandler(true, false, opex.Message);
            }
        }
    }
}
