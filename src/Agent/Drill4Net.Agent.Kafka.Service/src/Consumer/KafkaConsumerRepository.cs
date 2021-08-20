using System.IO;
using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Kafka.Service
{
    public class KafkaConsumerRepository : AbstractRepository<ConverterOptions>
    {
        public KafkaConsumerRepository() : base(CoreConstants.SUBSYSTEM_AGENT)
        {
            var optHelper = new BaseOptionsHelper<ConverterOptions>();
            var path = Path.Combine(FileUtils.GetExecutionDir(), CoreConstants.CONFIG_SERVICE_NAME);
            Options = optHelper.ReadOptions(path);

            PrepareLogger();
        }
    }
}
