using System;
using System.Threading;
using Confluent.Kafka;
using Drill4Net.Repository;
using Drill4Net.BanderLog;
using Drill4Net.BanderLog.Sinks.File;
using Drill4Net.BanderLog.Sinks.Console;

namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    public class ProbeKafkaReceiver : AbstractKafkaReceiver<MessagerOptions>, IProbeReceiver
    {
        public event ProbeReceivedHandler ProbeReceived;

        private readonly Logger _logger;
        private CancellationTokenSource _cts;

        /****************************************************************************************/

        public ProbeKafkaReceiver(AbstractRepository<MessagerOptions> rep) : base(rep)
        {
            PrepareDefaultLogger();
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
            _logger.Debug($"Probe servers: {string.Join(",", _rep.Options.Servers)}");

            _cts = new();
            var opts = _rep.Options;
            var probeTopics = MessagingUtils.FilterProbeTopics(opts.Receiver.Topics);
            _logger.Debug($"Probe topics: {string.Join(",", probeTopics)}");

            while (true)
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
                                _logger.Trace($"Probe is retrieved: [{probe}]"); //TEST
                                try
                                {
                                    ProbeReceived?.Invoke(probe);
                                }
                                catch (Exception ex)
                                {
                                    var mess = $"Processing of probe failed. Probe = [{probe}].";
                                    _logger.Error(mess, ex);
                                    ErrorOccuredHandler(this, true, true, mess + " Error: " + ex.Message);
                                }
                            }
                            //Unknown topic (is not create by Server yet)
                            catch (ConsumeException e) when (e.HResult == -2146233088) { }
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
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error for init retrieving of probes", ex);
                    Thread.Sleep(2000); //yes, I think sync call is better, because the problem more likely is remote
                }
            }
        }

        public void PrepareDefaultLogger()
        {
            var path = LoggerHelper.GetCommonFilePath(LoggerHelper.LOG_FOLDER);
            var builder = new LogBuilder()
                .AddSink(new FileSink(path))
                .AddSink(new ConsoleSink()) //TODO: only for Debug or DebuggerAtached
                .Build();
            Log.Configure(builder);
        }
    }
}
