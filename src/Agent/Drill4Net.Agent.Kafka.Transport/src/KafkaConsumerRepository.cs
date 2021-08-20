using System.IO;
using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Kafka.Transport
{
    public class KafkaConsumerRepository : AbstractRepository<CommunicatorOptions>
    {
        public KafkaConsumerRepository() : base(CoreConstants.SUBSYSTEM_AGENT)
        {
            var optHelper = new BaseOptionsHelper<CommunicatorOptions>();
            var path = Path.Combine(FileUtils.GetExecutionDir(), CoreConstants.CONFIG_SERVICE_NAME);
            Options = optHelper.ReadOptions(path);

            PrepareLogger();
        }
    }
}
