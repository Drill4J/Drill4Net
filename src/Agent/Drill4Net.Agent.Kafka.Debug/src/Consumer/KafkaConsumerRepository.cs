using System.IO;
using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Kafka.Debug
{
    public class KafkaConsumerRepository : AbstractRepository<ConverterOptions>
    {
        public KafkaConsumerRepository() : base(ConverterConstants.SUBSYSTEM)
        {
            var optHelper = new BaseOptionsHelper<ConverterOptions>();
            var path = Path.Combine(FileUtils.GetExecutionDir(), ConverterConstants.CONFIG_NAME_DEFAULT);
            Options = optHelper.ReadOptions(path);

            PrepareLogger();
        }
    }
}
