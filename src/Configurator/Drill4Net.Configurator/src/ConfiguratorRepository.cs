using System.IO;
using Drill4Net.Common;
using Drill4Net.Repository;
using Drill4Net.Injector.Core;
using Drill4Net.Configuration;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.TestRunner.Core;

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

        public void SaveSystemConfiguration(SystemConfiguration cfg)
        {
            //Agent's options
            var agCfgPath = Path.Combine(Options.InstallDirectory, CoreConstants.CONFIG_NAME_DEFAULT);
            var agentOpts = ReadAgentOptions(agCfgPath);
            agentOpts.Admin.Url = cfg.AdminUrl;
            agentOpts.PluginDir = cfg.AgentPluginDirectory;
            WriteAgentOptions(agentOpts, agCfgPath);

            // Transmitter opts
            var transDir = Options.TransmitterDirectory;
            if (string.IsNullOrEmpty(transDir))
                transDir = @"..\transmitter";
            var transCfgPath = Path.Combine(transDir, CoreConstants.CONFIG_NAME_MIDDLEWARE);
            var transOpts = ReadMessagerOptions(transCfgPath);

            transOpts.Servers.Clear();
            transOpts.Servers.Add(cfg.MiddlewareUrl);

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

        //public void WriteInjectorOptions(InjectorOptions opts, string cfgPath)
        //{
        //    WriteOptions<InjectorOptions>(opts, cfgPath);
        //}

        //public void ReadInjectorOptions(InjectorOptions opts, string cfgPath)
        //{
        //    WriteOptions<InjectorOptions>(opts, cfgPath);
        //}

        public TestRunnerOptions ReadTestRunnerOptions(string cfgPath)
        {
            return ReadOptions<TestRunnerOptions>(cfgPath);
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
