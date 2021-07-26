using System;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    public class ProbeTransmitter
    {
        public static ProbeTransmitter Transmitter { get; }

        public IDataSender Sender { get; }

        /***********************************************************************************/

        static ProbeTransmitter()
        {
            var rep = new TransmitterRepository(); //just rep
            IDataSender sender = new KafkaSender(rep); //concrete sender the data of probes to the middleware (Kafka)
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
        /// <param name="str">The cross-point data.</param>
        public static void Transmit(string str)
        {
            Transmitter.SendProbe(str);
        }

        /// <summary>
        /// Sends the specified probe to the middleware.
        /// </summary>
        /// <param name="str">The cross-point data.</param>
        public int SendProbe(string str)
        {
            return Sender.SendProbe(str);
        }
    }
}
