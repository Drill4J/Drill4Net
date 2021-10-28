using System;
using System.Linq;
using System.Threading;
using Confluent.Kafka;
using Drill4Net.BanderLog;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    public class ProbeKafkaReceiver : AbstractKafkaReceiver<MessageReceiverOptions>, IProbeReceiver
    {
        public event ProbeReceivedHandler ProbeReceived;

        private readonly Logger _logger;
        private CancellationTokenSource _cts;

        /****************************************************************************************/

        public ProbeKafkaReceiver(AbstractRepository<MessageReceiverOptions> rep) : base(rep)
        {
            _logger = new TypedLogger<ProbeKafkaReceiver>(rep.Subsystem);
        }

        /****************************************************************************************/

        public override void Start()
        {
            Stop();
            IsStarted = true;
            _logger.Debug("Start.");
            RetrieveProbes();
        }

        public override void Stop()
        {
            if (!IsStarted)
                return;
            IsStarted = false;
            _logger.Debug("Stop.");
            if (_cts?.Token.IsCancellationRequested == false)
                _cts.Cancel();
        }

        private void RetrieveProbes()
        {
            _logger.Info("Start retrieving probes...");

            _cts = new();
            var opts = _rep.Options;
            var probeTopics = MessagingUtils.FilterProbeTopics(opts.Topics);
            _logger.Debug($"Probe topics: {string.Join(",", probeTopics)}");

            while (true) //?
            {
                try
                {
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
                                var cr = c.Consume(_cts.Token);
                                var probe = cr.Message.Value;
                                ProbeReceived?.Invoke(probe);
                            }
                            catch (ConsumeException e)
                            {
                                var err = e.Error;
                                ErrorOccuredHandler(this, err.IsFatal, err.IsLocalError, err.Reason);
                            }
                        }
                    }
                    catch (OperationCanceledException opex)
                    {
                        // Ensure the consumer leaves the group cleanly and final offsets are committed.
                        c.Close();

                        _logger.Warning("Consuming was cancelled", opex);
                        ErrorOccuredHandler(this, true, false, opex.Message);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error for init retrieving of probes", ex);
                    Thread.Sleep(2000); //yes, I think sync call is better, because the problem more likely is remote
                }
            }
        }
    }
}
