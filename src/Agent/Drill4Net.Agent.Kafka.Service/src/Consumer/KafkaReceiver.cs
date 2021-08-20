using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using Drill4Net.Common;
using Drill4Net.Agent.Kafka.Common;

namespace Drill4Net.Agent.Kafka.Service
{
    //https://github.com/patsevanton/docker-compose-kafka-zk-kafdrop-cmak/blob/main/docker-compose.yml

    public delegate void ProbeReceivedHandler(Probe probe);
    public delegate void TargetReceivedInfoHandler(TargetInfo target);
    public delegate void ErrorOccuredHandler(bool isFatal, bool isLocal, string message);

    /***************************************************************************************************************/

    public class KafkaReceiver : IProbeReceiver
    {
        public event ProbeReceivedHandler ProbeReceived;
        public event TargetReceivedInfoHandler TargetInfoReceived;
        public event ErrorOccuredHandler ErrorOccured;

        private CancellationTokenSource _targetsCts;
        private CancellationTokenSource _probesCts;

        private readonly ConsumerConfig _cfg;
        private readonly AbstractRepository<ConverterOptions> _rep;

        /****************************************************************************************/

        public KafkaReceiver(AbstractRepository<ConverterOptions> rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            var opts = _rep.Options;
            _cfg = new ConsumerConfig
            {
                GroupId = opts.GroupId,
                BootstrapServers = string.Join(",", opts.Servers),

                // Note: The AutoOffsetReset property determines the start offset in the event
                // there are not yet any committed offsets for the consumer group for the
                // topic/partitions of interest. By default, offsets are committed
                // automatically, so in this case, consumption will only start from the
                // earliest message in the topic the first time you run the program.
                AutoOffsetReset = AutoOffsetReset.Earliest,

                EnableAutoCommit = true,
                EnableAutoOffsetStore = true,
                MessageMaxBytes = KafkaConstants.MaxMessageSize,
            };
        }

        /****************************************************************************************/

        public void Start()
        {
            Task.Run(RetriveTargets);
            RetriveProbes();
        }

        private void RetriveTargets()
        {
            var opts = _rep.Options;
            _targetsCts = new();
            var targets = new Dictionary<Guid, List<byte[]>>();

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
                            ErrorOccured?.Invoke(true, true, ex.Message);
                        }
                    }
                    catch (ConsumeException e)
                    {
                        var err = e.Error;
                        ErrorOccured?.Invoke(err.IsFatal, err.IsLocalError, err.Reason);
                    }
                }
            }
            catch (OperationCanceledException opex)
            {
                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                c.Close();

                ErrorOccured?.Invoke(true, false, opex.Message);
            }
        }

        private void RetriveProbes()
        {
            var opts = _rep.Options;
            _probesCts = new();

            using var c = new ConsumerBuilder<Ignore, Probe>(_cfg).Build();
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
                        ErrorOccured?.Invoke(err.IsFatal, err.IsLocalError, err.Reason);
                    }
                }
            }
            catch (OperationCanceledException opex)
            {
                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                c.Close();

                ErrorOccured?.Invoke(true, false, opex.Message);
            }
        }

        public void Stop()
        {
            _targetsCts.Cancel();
            _probesCts.Cancel();
        }
    }
}
