﻿using System;
using System.Linq;
using System.Collections.Generic;
using Confluent.Kafka;
using Drill4Net.Agent.Kafka.Common;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    public class KafkaSender : IDataSender
    {
        public bool IsError { get; private set; }

        public string LastError { get; private set; }

        public bool IsFatalError { get; private set; }

        private Headers _infoHeaders;
        private Headers _probeHeaders;

        private List<string> _probeTopics;
        private readonly int _packetMaxSize;

        private IProducer<Null, string> _probeProducer;
        private IProducer<Null, byte[]> _infoProducer;

        private readonly ProducerConfig _cfg;
        private readonly TransmitterRepository _rep;

        /***************************************************************************************/

        public KafkaSender(TransmitterRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _cfg = CreateProducerConfig(rep);

            //https://stackoverflow.com/questions/21020347/how-can-i-send-large-messages-with-kafka-over-15mb
            _packetMaxSize = (_cfg.MessageMaxBytes ?? KafkaConstants.MaxMessageSize) - 512; //less because also service info included

            CreateProbeHeaders();
            CreateTargetHeaders();

            CreateProducers();
        }

        ~KafkaSender()
        {
            _probeProducer?.Flush(TimeSpan.FromSeconds(10));
            _probeProducer?.Dispose();
        }

        /***************************************************************************************/

        public int SendProbe(string str)
        {
            foreach (var topic in _probeTopics)
            {
                var mess = new Message<Null, string> { Value = str, Headers = _probeHeaders };
                _probeProducer.Produce(topic, mess, HandleStringData);
            }

            if (!IsError) //if there is no connection, it will come here without an error :(
                return 0;
            return IsFatalError ? -2 : -1;
        }

        public int SendTargetInfo(byte[] info)
        {
            SetHeaderValue(_infoHeaders, KafkaConstants.HEADER_REQUEST, _rep.Session);

            var data = Compressor.Compress(info);

            //if needed break down array to chunks by size of _infoHeaders and
            //send separately transactionally
            //TODO: transactionally!!!
            var len = data.Length;
            if (len <= _packetMaxSize)
            {
                SetHeaderValue(_infoHeaders, KafkaConstants.HEADER_MESSAGE_PACKETS, 1);
                SetHeaderValue(_infoHeaders, KafkaConstants.HEADER_MESSAGE_PACKET, 0);
                return SendPacket(data);
            }
            else
            {
                var packetCnt = len / _packetMaxSize + 1;
                SetHeaderValue(_infoHeaders, KafkaConstants.HEADER_MESSAGE_PACKETS, packetCnt);

                for (var i = 0; i < packetCnt; i++)
                {
                    var packet = new byte[_packetMaxSize];
                    var start = i * _packetMaxSize;
                    Array.Copy(data, start, packet, 0, Math.Min(_packetMaxSize, len - start));

                    SetHeaderValue(_infoHeaders, KafkaConstants.HEADER_MESSAGE_PACKET, i);
                    SendPacket(packet);
                }
            }
            return LastError == null ? 0 : -2;
        }

        private int SendPacket(byte[] packet)
        {
            var mess = new Message<Null, byte[]> { Value = packet, Headers = _infoHeaders };
            _infoProducer.Produce(KafkaConstants.TOPIC_TARGET_INFO, mess, HandleBytesData);
            return LastError == null ? 0 : -2;
        }

        private void SetHeaderValue(Headers headers, string key, object val)
        {
            headers.Remove(key);
            var header = new Header(key, _rep.Serialize(val));
            headers.Add(header);
        }

        private ProducerConfig CreateProducerConfig(TransmitterRepository rep)
        {
            var transOpts = rep.TransmitterOptions;
            return new ProducerConfig
            {
                BootstrapServers = string.Join(",", transOpts.Servers),
                MessageMaxBytes = KafkaConstants.MaxMessageSize,
            };
        }

        private void CreateProducers()
        {
            _infoProducer = new ProducerBuilder<Null, byte[]>(_cfg).Build();

            _probeTopics = _rep.TransmitterOptions.Topics;
            _probeProducer = new ProducerBuilder<Null, string>(_cfg).Build();
        }

        #region Handle sending
        private void HandleBytesData(DeliveryReport<Null, byte[]> report)
        {
            Handle(report.Error);
        }

        private void HandleStringData(DeliveryReport<Null, string> report)
        {
            Handle(report.Error);
        }

        private void Handle(Error err)
        {
            IsError = err.IsError;
            IsFatalError = err.IsFatal;
            LastError = err.Reason;
        }
        #endregion
        #region Headers
        internal void CreateTargetHeaders()
        {
            _infoHeaders = GetCommonHeaders();
            _infoHeaders.Add(new Header(KafkaConstants.HEADER_MESSAGE_TYPE, _rep.StringToBytes(KafkaConstants.MESSAGE_TYPE_TARGET_INFO)));
        }

        internal void CreateProbeHeaders()
        {
            _probeHeaders = GetCommonHeaders();
            _probeHeaders.Add(new Header(KafkaConstants.HEADER_MESSAGE_TYPE, _rep.StringToBytes(KafkaConstants.MESSAGE_TYPE_PROBE)));
        }

        internal Headers GetCommonHeaders()
        {
            return new Headers
            {
                new Header(KafkaConstants.HEADER_SUBSYSTEM, _rep.StringToBytes(_rep.Subsystem)),
                new Header(KafkaConstants.HEADER_TARGET, _rep.StringToBytes(_rep.Target)),
            };
        }
        #endregion
    }
}
