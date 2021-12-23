﻿using System.IO;
using Drill4Net.Common;
using Drill4Net.Repository;
using Drill4Net.Injector.Core;
using Drill4Net.Configuration;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Messaging;

namespace Drill4Net.Configurator
{
    public class ConfiguratorRepository : AbstractRepository<ConfiguratorOptions>
    {
        public ConfiguratorRepository(): this(null) { }

        public ConfiguratorRepository(ConfiguratorOptions? opts) : base(CoreConstants.SUBSYSTEM_CONFIGURATOR, false)
        {
            Options = opts ?? ReadOptions<ConfiguratorOptions>(Path.Combine(FileUtils.ExecutingDir, CoreConstants.CONFIG_NAME_APP));
        }

        /***********************************************************************************/

        public void SaveSystemOptions(SystemOptions opts)
        {
            //Agent's options
            var agCfgPath = Path.Combine(Options.InstallDirectory, CoreConstants.CONFIG_NAME_DEFAULT);
            var agentOpts = ReadAgentOptions(agCfgPath);
            agentOpts.Admin.Url = opts.AdminUrl;
            agentOpts.PluginDir = opts.AgentPluginDirectory;
            WriteAgentOptions(agentOpts, agCfgPath);

            // Transmitter opts
            var transCfgPath = Path.Combine(Options.TransmitterDirectory, CoreConstants.CONFIG_NAME_MIDDLEWARE);
            var transOpts = ReadMessagerOptions(transCfgPath);

            transOpts.Servers.Clear();
            transOpts.Servers.Add(opts.MiddlewareUrl);

            WriteMessagerOptions(transOpts, transCfgPath);
        }

        public AgentOptions ReadAgentOptions(string cfgPath)
        {
            return ReadOptions<AgentOptions>(cfgPath);
        }

        public void WriteAgentOptions(AgentOptions opts, string cfgPath)
        {
            WriteOptions<AgentOptions>(opts, cfgPath);
        }

        public MessagerOptions ReadMessagerOptions(string cfgPath)
        {
            return ReadOptions<MessagerOptions>(cfgPath);
        }

        public void WriteMessagerOptions(MessagerOptions opts, string cfgPath)
        {
            WriteOptions<MessagerOptions>(opts, cfgPath);
        }

        public InjectorOptions ReadInjectorOptions(string cfgPath)
        {
            return ReadOptions<InjectorOptions>(cfgPath);
        }

        public void ReadInjectorOptions(InjectorOptions opts, string cfgPath)
        {
            WriteOptions<InjectorOptions>(opts, cfgPath);
        }

        public T ReadOptions<T>(string cfgPath) where T : AbstractOptions, new()
        {
            var optHelper = new BaseOptionsHelper<T>(Subsystem);
            return optHelper.ReadOptions(cfgPath);
        }

        public void WriteOptions<T>(T opts, string cfgPath) where T : AbstractOptions, new()
        {
            var optHelper = new BaseOptionsHelper<T>(Subsystem);
            optHelper.WriteOptions(opts, cfgPath);
        }
    }
}
