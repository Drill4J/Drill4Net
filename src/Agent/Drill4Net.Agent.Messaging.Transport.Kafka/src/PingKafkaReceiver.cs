using System;
using System.Threading;
using System.Collections.Specialized;
using Confluent.Kafka;
using Drill4Net.Common;
using Drill4Net.Agent.Messaging.Kafka;

namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    public class PingKafkaReceiver : AbstractKafkaReceiver, IPingReceiver
    {
        public event PingReceivedHandler PingReceived;

        private CancellationTokenSource _cts;

        /*******************************************************************************/

        public PingKafkaReceiver(AbstractRepository<MessageReceiverOptions> rep,
            CancellationTokenSource cts = null) : base(rep)
        {
            _cts = cts;
            _cfg.AutoOffsetReset = AutoOffsetReset.Latest; //TODO: IT'S DON'T WORK!!!! AAAAA!!!!
        }

        /*******************************************************************************/

        public override void Start()
        {
            RetrievePings();
        }

        public override void Stop()
        {
            _cts?.Cancel();
        }

        private void RetrievePings()
        {
            Console.WriteLine($"{_logPrefix}Starting retrieving pings...");

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
                            ErrorOccuredHandler(true, true, ex.Message);
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

                ErrorOccuredHandler(true, false, opex.Message);
            }
        }
    }
}