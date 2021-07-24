using System;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    public class ProbeTransmitter
    {
        private readonly IProbeSender _sender;

        /*************************************************************************/

        public ProbeTransmitter(IProbeSender sender)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        /*************************************************************************/

        public int Send(string str)
        {
            return _sender.Send(str);
        }
    }
}
