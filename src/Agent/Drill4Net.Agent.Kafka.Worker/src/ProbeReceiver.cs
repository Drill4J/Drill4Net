using System;
using System.Linq;
using System.Threading;
using Confluent.Kafka;
using Drill4Net.Common;
using Drill4Net.Agent.Kafka.Common;
using Drill4Net.Agent.Kafka.Transport;

namespace Drill4Net.Agent.Kafka.Worker
{
    public delegate void ProbeReceivedHandler(Probe probe);

    /********************************************************************************************/

    public class ProbeReceiver : AbstractKafkaReceiver, IProbeReceiver
    {
        public event ProbeReceivedHandler ProbeReceived;

        private CancellationTokenSource _probesCts;

        /****************************************************************************************/

        public ProbeReceiver(AbstractRepository<MessageReceiverOptions> rep) : base(rep)
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
            Console.WriteLine("Starting retrieving probes...");

            var opts = _rep.Options;
            _probesCts = new();

            using var c = new ConsumerBuilder<Ignore, Probe>(_cfg)
                .SetValueDeserializer(new ProbeDeserializer())
                .Build();
            c.Subscribe(opts.Topics);

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
