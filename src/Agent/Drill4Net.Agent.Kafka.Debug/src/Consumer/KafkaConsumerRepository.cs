using System;
using System.Linq;
using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Kafka.Debug
{
    public class KafkaConsumerRepository : AbstractRepository<ConverterOptions>
    {
        /******************************************************************/

        public KafkaConsumerRepository() : base(ConverterConstants.SUBSYSTEM)
        {
            var optHelper = new BaseOptionsHelper<ConverterOptions>();
            Options = optHelper.ReadOptions(ConverterConstants.CONFIG_NAME_DEFAULT);

            PrepareLogger();
        }

        /******************************************************************/
    }
}
