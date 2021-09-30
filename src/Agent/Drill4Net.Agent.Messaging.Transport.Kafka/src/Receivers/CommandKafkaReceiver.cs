using System;
using System.Linq;
using System.Threading;
using Confluent.Kafka;
using Drill4Net.BanderLog;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    public class CommandKafkaReceiver : AbstractKafkaReceiver<MessageReceiverOptions>, ICommandReceiver
    {
        public event CommandReceivedHandler CommandReceived;

        private readonly Logger _logger;
        private CancellationTokenSource _cts;

        /****************************************************************************************/

        public CommandKafkaReceiver(AbstractRepository<MessageReceiverOptions> rep) : base(rep)
        {
            _logger = new TypedLogger<CommandKafkaReceiver>(rep.Subsystem);
        }

        /****************************************************************************************/

        public override void Start()
        {
            Stop();
            IsStarted = true;
            _logger.Debug("Start.");
            RetrieveCommands();
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

        private void RetrieveCommands()
        {
            _logger.Info("Start retrieving commands...");

            _cts = new();
            var opts = _rep.Options;
            var probeTopics = MessagingUtils.FilterProbeTopics(opts.Topics);
            _logger.Debug($"Command topics: {string.Join(",", probeTopics)}");

            try
            {
                using var c = new ConsumerBuilder<Ignore, Command>(_cfg)
                    .SetValueDeserializer(new CommandDeserializer())
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
                            CommandReceived?.Invoke(probe);
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
                }
            }
            catch (Exception ex)
            {
                _logger.Fatal("Error for init retrieving of commands", ex);
            }
        }
    }
}
