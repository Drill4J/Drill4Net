using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    public class KafkaSenderRepository : AbstractRepository<TransmitterOptions>
    {
        public KafkaSenderRepository() : base(TransmitterConstants.SUBSYSTEM)
        {
            var optHelper = new BaseOptionsHelper<TransmitterOptions>();
            Options = optHelper.ReadOptions(TransmitterConstants.CONFIG_NAME_DEFAULT);

            PrepareLogger();
        }
    }
}
