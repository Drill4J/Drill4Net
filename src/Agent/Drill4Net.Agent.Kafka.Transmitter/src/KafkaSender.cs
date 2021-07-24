using Drill4Net.Common;
using System;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    public class KafkaSender : IProbeSender
    {
        private readonly AbstractRepository<TransmitterOptions> _rep;

        /****************************************************************************/

        public KafkaSender(AbstractRepository<TransmitterOptions> rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
        }

        /****************************************************************************/

        public int Send(string str)
        {
            return -1;
        }
    }
}
