using System;
using System.IO;
using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Messaging.Transport
{
    public class MessageReceiverRepository : AbstractRepository<MessageReceiverOptions>
    {
        public MessageReceiverRepository(string subsystem, string cfgPath = null): this(subsystem, GetOptionsByPath(cfgPath))
        {
        }

        public MessageReceiverRepository(string subsystem, MessageReceiverOptions opts): base(subsystem)
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
