using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using Drill4Net.Common;
using Drill4Net.Agent.Kafka.Common;
using Drill4Net.Agent.Kafka.Transport;

namespace Drill4Net.Agent.Kafka.Service
{
    //https://github.com/patsevanton/docker-compose-kafka-zk-kafdrop-cmak/blob/main/docker-compose.yml

    public delegate void TargetReceivedInfoHandler(TargetInfo target);

    /********************************************************************************************/

    public class KafkaServerReceiver : AbstractKafkaReceiver, IKafkaServerReceiver
    {
        public event TargetReceivedInfoHandler TargetInfoReceived;

        private CancellationTokenSource _targetsCts;

        /****************************************************************************************/

        public KafkaServerReceiver(AbstractRepository<CommunicatorOptions> rep, CancellationTokenSource targetsCts = null) : base(rep)
        {
            _targetsCts = targetsCts;
        }

        /****************************************************************************************/

        public override void Start()
        {
            Task.Run(RetriveTargets);
        }

        public override void Stop()
        {
            _targetsCts?.Cancel();
        }

        private void RetriveTargets()
        {
            var opts = _rep.Options;
            var targets = new Dictionary<Guid, List<byte[]>>();
            if (_targetsCts == null)
                _targetsCts = new();

            using var c = new ConsumerBuilder<Ignore, byte[]>(_cfg).Build();
            c.Subscribe(KafkaConstants.TOPIC_TARGET_INFO);

            try
            {
                while (true)
                {
                    try
                    {
                        var cr = c.Consume(_targetsCts.Token);
                        var mess = cr.Message;
                        var headers = mess.Headers;
                        var packet = mess.Value;
                        try
                        {
                            if (!headers.TryGetLastBytes(KafkaConstants.HEADER_REQUEST, out byte[] uidAr))
                                throw new Exception("No Uid in packet header");
                            var uid = Serializer.FromArray<Guid>(uidAr);

                            if (!headers.TryGetLastBytes(KafkaConstants.HEADER_MESSAGE_PACKETS, out byte[] packetsCntAr))
                                throw new Exception("No packets count in packet header");
                            var packetsCnt = Serializer.FromArray<int>(packetsCntAr);

                            if (!headers.TryGetLastBytes(KafkaConstants.HEADER_MESSAGE_PACKET, out byte[] packetIndAr))
                                throw new Exception("No packet's index in packet header");
                            var packetInd = Serializer.FromArray<int>(packetIndAr);

                            //add packet
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

                            //end?
                            if (packetInd == packetsCnt - 1)
                            {
                                // merging packets
                                if (!headers.TryGetLastBytes(KafkaConstants.HEADER_MESSAGE_COMPRESSED_SIZE, out byte[] messSizeAr))
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
                                if (!headers.TryGetLastBytes(KafkaConstants.HEADER_MESSAGE_DECOMPRESSED_SIZE, out messSizeAr))
                                    throw new Exception("No decompressed message size in packet header");
                                messSize = Serializer.FromArray<int>(messSizeAr);

                                var decompressed = Compressor.Decompress(messAr, messSize);
                                var info = Serializer.FromArray<TargetInfo>(decompressed);
                                targets.Remove(uid);
                                GC.Collect();

                                TargetInfoReceived?.Invoke(info);
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorOccuredHandler(true, true, ex.Message);
                        }
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
