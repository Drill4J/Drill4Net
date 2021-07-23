using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Agent.Kafka.Debug
{
    public class KafkaConsumerRepository
    {
        public ConverterOptions Options { get; set; }

        private const string CFG_NAME_DEFAULT = "svc.yml";

        /******************************************************************/

        public KafkaConsumerRepository()
        {
        }

        /******************************************************************/
    }
}
