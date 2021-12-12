﻿using System;
using Confluent.Kafka;
using Drill4Net.Common;

namespace Drill4Net.Agent.Messaging.Kafka
{
    public class TargetInfoKafkaSender : AbstractKafkaSender, ITargetInfoSender
    {
        private IProducer<Null, byte[]> _targetProducer;

        /**************************************************************************/

        public TargetInfoKafkaSender(ITargetedInfoSenderRepository rep): base(rep)
        {
        }

        /**************************************************************************/

        protected override void CreateProducers()
        {
            _targetProducer = new ProducerBuilder<Null, byte[]>(_cfg).Build();
        }

        public int SendTargetInfo(byte[] info, string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                topic = MessagingConstants.TOPIC_TARGET_INFO;

            SetHeaderValue<Guid>(MessagingConstants.HEADER_REQUEST, _rep.TargetSession);
            SetHeaderValue<int>(MessagingConstants.HEADER_MESSAGE_DECOMPRESSED_SIZE, info.Length);

            var data = Compressor.Compress(info);

            //TEST
            //var data2 = Compressor.Decompress(data, info.Length);
            //var aaa = _rep.Deserialize(data2) as TargetInfo; // 

            //if needed break down array to chunks by size of _infoHeaders and
            //send separately transactionally
            //TODO: transactionally!!!

            var len = data.Length;
            SetHeaderValue<int>(MessagingConstants.HEADER_MESSAGE_COMPRESSED_SIZE, len);
            if (len <= _packetMaxSize)
            {
                SetHeaderValue<int>(MessagingConstants.HEADER_MESSAGE_PACKET_CNT, 1);
                SetHeaderValue<int>(MessagingConstants.HEADER_MESSAGE_PACKET_IND, 0);
                return SendPacket(data, topic);
            }
            else
            {
                var packetCnt = len / _packetMaxSize + 1;
                SetHeaderValue<int>(MessagingConstants.HEADER_MESSAGE_PACKET_CNT, packetCnt);

                for (var i = 0; i < packetCnt; i++)
                {
                    var start = i * _packetMaxSize;
                    var size = Math.Min(_packetMaxSize, len - start);
                    var packet = new byte[size];
                    Array.Copy(data, start, packet, 0, size);

                    SetHeaderValue<int>(MessagingConstants.HEADER_MESSAGE_PACKET_IND, i);
                    SendPacket(packet, topic);
                }
            }
            return LastError == null ? 0 : -2;
        }

        private int SendPacket(byte[] packet, string topic = null)
        {
            if (string.IsNullOrWhiteSpace(topic))
                topic = MessagingConstants.TOPIC_TARGET_INFO;
            //
            var mess = new Message<Null, byte[]> { Value = packet, Headers = _headers };
            _targetProducer.Produce(topic, mess, HandleBytesData);
            _targetProducer.Flush(new TimeSpan(0,0,5));
            return LastError == null ? 0 : -2;
        }

        private void HandleBytesData(DeliveryReport<Null, byte[]> report)
        {
            Handle(report.Error);
        }

        protected override string GetMessageType()
        {
            return MessagingConstants.MESSAGE_TYPE_TARGET_INFO;
        }

        protected override void ConcreteDisposing()
        {
            _targetProducer.Flush();
            _targetProducer?.Dispose();
        }
    }
}
