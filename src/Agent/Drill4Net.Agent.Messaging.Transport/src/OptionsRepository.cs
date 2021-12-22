using System;
using System.IO;
using Drill4Net.Common;
using Drill4Net.Repository;

namespace Drill4Net.Agent.Messaging.Transport
{
    public class OptionsRepository<T> : AbstractRepository<T> where T: MessagerOptions, new()
    {
        public OptionsRepository(string subsystem, string cfgPath = null):
            this(subsystem, GetOptionsByPath(subsystem, cfgPath))
        {
        }

        public OptionsRepository(string subsystem, T opts): base(subsystem)
        {
            Options = opts ?? throw new ArgumentNullException(nameof(opts));
            PrepareLogger();
        }

        /************************************************************************************************/

        public static T GetOptionsByPath(string subsystem, string cfgPath = null)
        {
             var optHelper = new BaseOptionsHelper<T>(subsystem);
             if(string.IsNullOrWhiteSpace(cfgPath))
                cfgPath = Path.Combine(FileUtils.ExecutingDir, CoreConstants.CONFIG_NAME_MIDDLEWARE);
             return optHelper.ReadOptions(cfgPath);
        }
    }
}
