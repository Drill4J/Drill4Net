using System;
using System.IO;
using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Kafka.Transport
{
    public class KafkaReceiverRepository : AbstractRepository<MessageReceiverOptions>
    {
        public KafkaReceiverRepository(string cfgPath = null): this(GetOptionsByPath(cfgPath))
        {
        }

        public KafkaReceiverRepository(MessageReceiverOptions opts): base(CoreConstants.SUBSYSTEM_AGENT)
        {
            Options = opts ?? throw new ArgumentNullException(nameof(opts));
            PrepareLogger();
        }

        /************************************************************************************************/

        public static MessageReceiverOptions GetOptionsByPath(string cfgPath = null)
        {
             var optHelper = new BaseOptionsHelper<MessageReceiverOptions>();
             if(string.IsNullOrWhiteSpace(cfgPath))
                cfgPath = Path.Combine(FileUtils.GetExecutionDir(), CoreConstants.CONFIG_SERVICE_NAME);
             return optHelper.ReadOptions(cfgPath);
        }
    }
}
