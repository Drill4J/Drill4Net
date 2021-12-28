using System;
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

        public string GetAppPath()
        {
            return Path.Combine(FileUtils.EntryDir, "dfn.exe");
        }

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

        public CiOptions ReadCiOptions(string cfgPath, bool validate = true)
        {
            var opts = ReadOptions<CiOptions>(cfgPath);

            //validate
            if (validate)
            {
                var cfgDirs = opts.Injection?.ConfigDir;
                if (string.IsNullOrWhiteSpace(cfgDirs) || !Directory.Exists(cfgDirs))
                    throw new ArgumentException("The directory for injector's configs does not exist");

                var runnerCfg = opts.TestRunnerConfigPath;
                if (string.IsNullOrWhiteSpace(runnerCfg) || !File.Exists(runnerCfg))
                    throw new ArgumentException("The Test Runner's config does not exist");
            }
            //
            return opts;
        }

        public void WriteCiOptions(CiOptions opts, string cfgPath)
        {
            WriteOptions<CiOptions>(opts, cfgPath);
        }

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
