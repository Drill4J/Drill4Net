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

        /// <summary>
        /// It is used for the Docker environment - overrides the options for middleware address
        /// </summary>
        /// <param name="servers"></param>
        /// <returns></returns>
        internal static bool GetServerAddressesFromEnvVars(out List<string> servers)
        {
            servers = new();
            var val = Environment.GetEnvironmentVariable(CoreConstants.ENV_MESSAGE_SERVER_ADDRESS, EnvironmentVariableTarget.Process);
            if (val == null)
            {
                _logger.Info($"The environment variables for message server address is emty - will be used the config's value");
                return false;
            }
            servers = val.Split(',').Select(a => a.Trim()).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
            var mess = "Message server address found in the environment variables";
            if (servers.Count == 0)
            {
                _logger.Error($"{mess}: {servers}");
                return false;
            }
            _logger.Info($"{mess}, but no address: {servers}");
            return true;
        }
    }
}
