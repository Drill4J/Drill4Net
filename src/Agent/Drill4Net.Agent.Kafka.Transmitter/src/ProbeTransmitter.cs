﻿using Drill4Net.Common;
using System;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    /// <summary>
    /// Entity which is loaded by Proxy to Target's process and next transmits
    /// the probe's data from it to the real Agent located in separate service 
    /// (direct or, e.g. through Kafka as middleware)
    /// </summary>
    public class ProbeTransmitter
    {
        public static ProbeTransmitter Transmitter { get; }

        public IDataSender Sender { get; }

        /***********************************************************************************/

        static ProbeTransmitter()
        {
            var rep = new TransmitterRepository(); //just rep
            IDataSender sender = new KafkaTransmitterSender(rep); //concrete sender the data of probes to the middleware (Kafka)
            Transmitter = new ProbeTransmitter(sender); //what is loaded into the Target process and used by the Proxy class

            Transmitter.SendTargetInfo(rep.GetTargetInfo());
        }

        public ProbeTransmitter(IDataSender sender)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        /************************************************************************************/

        internal void SendTargetInfo(byte[] info)
        {
            Sender.SendTargetInfo(info);
        }

        /// <summary>
        /// Transmits the specified probe from the Proxy class injected into Target to the middleware.
        /// </summary>
        /// <param name="data">The cross-point data.</param>
        public static void Transmit(string data)
        {
            var ctx = Contexter.GetContextId();
            Transmitter.SendProbe(data, ctx);
        }

        /// <summary>
        /// Sends the specified probe to the middleware.
        /// </summary>
        /// <param name="data">The cross-point data.</param>
        /// <param name="ctx">The context of data (user, process, worker, etc)</param>
        public int SendProbe(string data, string ctx)
        {
            return Sender.SendProbe(data, ctx);
        }
    }
}
