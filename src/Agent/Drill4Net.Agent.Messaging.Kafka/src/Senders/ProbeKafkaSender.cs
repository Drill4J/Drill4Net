﻿using System;
using System.Collections.Generic;
using Confluent.Kafka;

namespace Drill4Net.Agent.Messaging.Kafka
{
    public class ProbeKafkaSender : AbstractKafkaSender, IProbeSender
    {
        private IProducer<Null, Probe> _probeProducer;
        private List<string> _probeTopics;

        /**************************************************************************/

        public ProbeKafkaSender(IMessagerRepository rep): base(rep)
        {
        }

        /**************************************************************************/

        public int SendProbe(string data, string ctx)
        {
            var probe = new Probe { Context = ctx, Data = data };
            foreach (var topic in _probeTopics)
            {
                var mess = new Message<Null, Probe> { Value = probe, Headers = _headers };
                _probeProducer.Produce(topic, mess, HandleProbeData);
            }

            if (!IsError) //if there is no connection, it will come here without an error :(
                return 0;
            return IsFatalError ? -2 : -1;
        }

        private void HandleProbeData(DeliveryReport<Null, Probe> report)
        {
            Handle(report.Error);
        }

        protected override void CreateProducers()
        {
            //topics
            _probeTopics = _rep.MessagerOptions.Sender.Topics ?? new List<string>(); //additional probe topics from cfg
            var baseProbeTopic = MessagingUtils.GetProbeTopic(_rep.TargetSession); //the base one
            _probeTopics.Add(baseProbeTopic);

            //producer
            _probeProducer = new ProducerBuilder<Null, Probe>(_cfg)
                .SetValueSerializer(new ProbeSerializer())
                .Build();
        }

        protected override string GetMessageType()
        {
            return MessagingConstants.MESSAGE_TYPE_PROBE;
        }

        public void Flush()
        {
            _probeProducer?.Flush(TimeSpan.FromSeconds(10));
        }

        protected override void ConcreteDisposing()
        {
            Flush();
            _probeProducer?.Dispose();
        }
    }
}
