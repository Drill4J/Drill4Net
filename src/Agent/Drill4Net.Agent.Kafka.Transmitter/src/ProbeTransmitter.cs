using System;
using Drill4Net.Common;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    public class ProbeTransmitter
    {
        public static ProbeTransmitter Transmitter { get; }

        public IProbeSender Sender { get; }

        /*************************************************************************/

        static ProbeTransmitter()
        {
            var rep = new TransmitterRepository(); //just rep
            IProbeSender sender = new KafkaProducer(rep); //concrete sender the data of probes to the middleware (Kafka)
            Transmitter = new ProbeTransmitter(sender); //what is loaded into the Target process and used by the Proxy class
        }

        public ProbeTransmitter(IProbeSender sender)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        /*************************************************************************/

        /// <summary>
        /// Transmits the specified probe by Proxy class injected into Target to the middleware.
        /// </summary>
        /// <param name="str">The cross-point data.</param>
        public static void Transmit(string str)
        {
            Transmitter.Send(str);
        }

        /// <summary>
        /// Sends the specified probe to the middleware.
        /// </summary>
        /// <param name="str">The cross-point data.</param>
        public int Send(string str)
        {
            return Sender.Send(str);
        }
    }
}
