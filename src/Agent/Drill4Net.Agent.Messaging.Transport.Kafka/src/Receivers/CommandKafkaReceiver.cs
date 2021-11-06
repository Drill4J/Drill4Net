using System;
using System.Linq;
using System.Threading;
using Confluent.Kafka;
using Drill4Net.BanderLog;

namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    public class CommandKafkaReceiver : AbstractKafkaReceiver<MessageReceiverOptions>, ICommandReceiver
    {
        public event CommandReceivedHandler CommandReceived;

        public Guid TargetSession { get; }

        private readonly Logger _logger;
        private CancellationTokenSource _cts;

        /****************************************************************************************/

        public CommandKafkaReceiver(TargetedReceiverRepository rep) : base(rep)
        {
            _logger = new TypedLogger<CommandKafkaReceiver>(rep.Subsystem);
            TargetSession = rep.TargetSession;
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
            var topics = MessagingUtils.GetCommandTopic(TargetSession);
            _logger.Debug($"Command topic: {topics}");

            while (true)
            {
                try
                {
                    using var c = new ConsumerBuilder<Ignore, Command>(_cfg)
                        .SetValueDeserializer(new CommandDeserializer())
                        .Build();
                    c.Subscribe(topics);

                    try
                    {
                        while (true)
                        {
                            try
                            {
                                var cr = c.Consume(_cts.Token);
                                var command = cr?.Message?.Value;
                                _logger.Debug($"Received command type: [{command?.Type}]");
                                CommandReceived?.Invoke(command);
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
                    _logger.Error("Error for init retrieving of commands", ex);
                    Thread.Sleep(2000); //yes, I think sync call is better, because the problem more likely is remote
                }
            }
        }
    }
}
