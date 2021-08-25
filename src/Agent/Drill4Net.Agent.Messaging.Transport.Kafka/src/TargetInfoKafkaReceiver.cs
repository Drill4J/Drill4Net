using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Confluent.Kafka;
using Drill4Net.Common;

namespace Drill4Net.Agent.Messaging.Transport.Kafka
{
    //https://github.com/patsevanton/docker-compose-kafka-zk-kafdrop-cmak/blob/main/docker-compose.yml

    public class TargetInfoKafkaReceiver<T> : AbstractKafkaReceiver<T>, ITargetInfoReceiver
        where T : MessageReceiverOptions, new()
    {
        public event TargetReceivedInfoHandler TargetInfoReceived;

        private CancellationTokenSource _cts;

        /****************************************************************************************/

        public TargetInfoKafkaReceiver(AbstractRepository<T> rep,
            CancellationTokenSource cts = null): base(rep)
        {
            _cts = cts;
        }

        /****************************************************************************************/

        public override void Start()
        {
            RetrieveTargets();
        }

        public override void Stop()
        {
            _cts?.Cancel();
        }

        private void RetrieveTargets()
        {
            Console.WriteLine($"{_logPrefix}Starting retrieving target info...");

            var targets = new Dictionary<Guid, List<byte[]>>();
            if (_cts == null)
                _cts = new();

            var opts = _rep.Options;
            var topics = TransportUtils.FilterTargetTopics(opts.Topics);
            Console.WriteLine($"{_logPrefix}Topics: {string.Join(",", topics)}");

            using var c = new ConsumerBuilder<Ignore, byte[]>(_cfg).Build();
            c.Subscribe(topics);

            try
            {
                while (true)
                {
                    try
                    {
                        var cr = c.Consume(_cts.Token);
                        var mess = cr.Message;
                        var headers = mess.Headers;
                        var packet = mess.Value;
                        try
                        {
                            #region Params
                            if (!headers.TryGetLastBytes(MessagingConstants.HEADER_REQUEST, out byte[] uidAr))
                                throw new Exception("No Uid in packet header");
                            var uid = Serializer.FromArray<Guid>(uidAr);

                            if (!headers.TryGetLastBytes(MessagingConstants.HEADER_MESSAGE_PACKETS, out byte[] packetsCntAr))
                                throw new Exception("No packets count in packet header");
                            var packetsCnt = Serializer.FromArray<int>(packetsCntAr);

                            if (!headers.TryGetLastBytes(MessagingConstants.HEADER_MESSAGE_PACKET, out byte[] packetIndAr))
                                throw new Exception("No packet's index in packet header");
                            var packetInd = Serializer.FromArray<int>(packetIndAr);
                            #endregion
                            #region Add packet
                            List<byte[]> packets;
                            if (targets.ContainsKey(uid))
                            {
                                packets = targets[uid];
                            }
                            else
                            {
                                packets = new List<byte[]>();
                                targets.Add(uid, packets);
                            }
                            packets.Add(packet);
                            #endregion
                            #region Data is collected
                            //end?
                            if (packetInd == packetsCnt - 1)
                            {
                                // merging packets
                                if (!headers.TryGetLastBytes(MessagingConstants.HEADER_MESSAGE_COMPRESSED_SIZE, out byte[] messSizeAr))
                                    throw new Exception("No compressed message size in packet header");
                                var messSize = Serializer.FromArray<int>(messSizeAr);
                                var messAr = new byte[messSize];

                                var start = 0;
                                foreach (var p in packets)
                                {
                                    var len = p.Length;
                                    Array.Copy(p, 0, messAr, start, len);
                                    start += len;
                                }

                                //decompression
                                if (!headers.TryGetLastBytes(MessagingConstants.HEADER_MESSAGE_DECOMPRESSED_SIZE, out messSizeAr))
                                    throw new Exception("No decompressed message size in packet header");
                                messSize = Serializer.FromArray<int>(messSizeAr);

                                var decompressed = Compressor.Decompress(messAr, messSize);
                                var info = Serializer.FromArray<TargetInfo>(decompressed);
                                targets.Remove(uid);
                                GC.Collect(1, GCCollectionMode.Forced);

                                TargetInfoReceived?.Invoke(info);
                            }
                            #endregion
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
