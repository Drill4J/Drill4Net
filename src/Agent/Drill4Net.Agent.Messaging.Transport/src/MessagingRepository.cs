using System;
using System.IO;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.Repository;
using Drill4Net.BanderLog;
using System.Linq;

namespace Drill4Net.Agent.Messaging.Transport
{
    public class MessagingRepository<TOpts> : AbstractRepository<TOpts> where TOpts: MessagerOptions, new()
    {
        private static Logger _logger;

        /************************************************************************************************/

        public MessagingRepository(string subsystem, string cfgPath = null):
            this(subsystem, GetOptionsByPath(subsystem, cfgPath))
        {
        }

        public MessagingRepository(string subsystem, TOpts opts): base(subsystem)
        {
            Options = opts ?? throw new ArgumentNullException(nameof(opts));
            //PrepareLogger();
            _logger = new TypedLogger<MessagingRepository<TOpts>>(Subsystem);
        }

        /************************************************************************************************/

        public static TOpts GetOptionsByPath(string subsystem, string cfgPath = null)
        {
            var optHelper = new BaseOptionsHelper<TOpts>(subsystem);
            if(string.IsNullOrWhiteSpace(cfgPath))
               cfgPath = Path.Combine(FileUtils.ExecutingDir, CoreConstants.CONFIG_NAME_MIDDLEWARE);
            var opts = optHelper.ReadOptions(cfgPath);
            if (GetServerAddressesFromEnvVars(out var envServers))
                opts.Servers = envServers;
            return opts;
        }

        internal static bool GetServerAddressesFromEnvVars(out List<string> servers)
        {
            servers = new();
            var val = Environment.GetEnvironmentVariable(CoreConstants.ENV_MESSAGE_SERVER_ADDRESS);
            if(val == null)
                return false;
            servers = val.Split(',').ToList();
            _logger.Info($"Message server address found in the environment variables: {servers}");
            return true;
        }
    }
}
