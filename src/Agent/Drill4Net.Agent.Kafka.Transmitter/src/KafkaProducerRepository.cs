using System.IO;
using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    public class KafkaProducerRepository : AbstractRepository<TransmitterOptions>
    {
        public KafkaProducerRepository() : base(TransmitterConstants.SUBSYSTEM)
        {
            var optHelper = new BaseOptionsHelper<TransmitterOptions>();
            var path = Path.Combine(FileUtils.GetExecutionDir(), TransmitterConstants.CONFIG_NAME_DEFAULT);
            Options = optHelper.ReadOptions(path);

            PrepareLogger();
        }
    }
}
