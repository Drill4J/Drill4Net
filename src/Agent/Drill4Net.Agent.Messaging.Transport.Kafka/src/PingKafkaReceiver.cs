using System;
using System.Threading;
using System.Collections.Specialized;
using Confluent.Kafka;
using Drill4Net.BanderLog;
using Drill4Net.Core.Repository;
using Drill4Net.Agent.Messaging.Kafka;

namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    public class PingKafkaReceiver<T> : AbstractKafkaReceiver<T>, IPingReceiver
        where T : MessageReceiverOptions, new()
    {
        public event PingReceivedHandler PingReceived;

        private readonly Logger _logger;
        private CancellationTokenSource _cts;

        /*******************************************************************************/

        public PingKafkaReceiver(AbstractRepository<T> rep, CancellationTokenSource cts = null) : base(rep)
        {
            _cts = cts;
            _logger = new TypedLogger<PingKafkaReceiver<T>>(rep.Subsystem);
            _cfg.AutoOffsetReset = AutoOffsetReset.Latest; //TODO: IT'S DON'T WORK!!!! AAAAA!!!!
        }

        /*******************************************************************************/

        public override void Start()
        {
            Stop();
            IsStarted = true;
            _logger.Debug("Start.");
            RetrievePings();
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

        private void RetrievePings()
        {
            _logger.Debug("Start retrieving pings...");

            if (_cts == null)
                _cts = new();

            using var c = new ConsumerBuilder<string, StringDictionary>(_cfg)
                .SetValueDeserializer(new StringDictionaryDeserializer())
                .Build();
            c.Subscribe(MessagingConstants.TOPIC_PING);

            try
            {
                while (true)
                {
                    try
                    {
                        var cr = c.Consume(_cts.Token);
                        var mess = cr.Message;
                        var sessionUid = mess.Key;
                        var pingData = mess.Value;
                        try
                        {
                            PingReceived?.Invoke(sessionUid, pingData);
                        }
                        catch (Exception ex)
                        {
                            ErrorOccuredHandler(this, true, true, ex.Message);
                        }
                    }
                    catch (ConsumeException e)
                    {
                        ProcessConsumeExcepton(e);
                    }
                }
            }
            catch (OperationCanceledException opex)
            {
                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                c.Close();

                ErrorOccuredHandler(this, true, false, opex.Message);
            }
        }
    }
}