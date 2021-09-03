using System;
using System.IO;
using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Messaging.Transport
{
    public class MessageReceiverRepository<T> : AbstractRepository<T> where T: MessageReceiverOptions, new()
    {
        public MessageReceiverRepository(string subsystem, string cfgPath = null):
            this(subsystem, GetOptionsByPath(cfgPath))
        {
        }

        public MessageReceiverRepository(string subsystem, T opts): base(subsystem)
        {
            Options = opts ?? throw new ArgumentNullException(nameof(opts));
            PrepareLogger();
        }

        /************************************************************************************************/

        public static T GetOptionsByPath(string cfgPath = null)
        {
             var optHelper = new BaseOptionsHelper<T>();
             if(string.IsNullOrWhiteSpace(cfgPath))
                cfgPath = Path.Combine(FileUtils.ExecutingDir, CoreConstants.CONFIG_SERVICE_NAME);
             return optHelper.ReadOptions(cfgPath);
        }
    }
}
