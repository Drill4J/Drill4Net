using System;
using System.IO;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Repository;
using Drill4Net.Injector.Core;
using Drill4Net.Configuration;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.TestRunner.Core;
using System.Collections.Generic;

namespace Drill4Net.Configurator
{
    public class ConfiguratorRepository : AbstractRepository<ConfiguratorOptions>
    {
        private readonly Logger _logger;

        /***********************************************************************************/

        public ConfiguratorRepository(): this(null) { }

        public ConfiguratorRepository(ConfiguratorOptions? opts) : base(CoreConstants.SUBSYSTEM_CONFIGURATOR, false)
        {
            _logger = new TypedLogger<ConfiguratorRepository>(Subsystem);
            Options = opts ?? ReadOptions<ConfiguratorOptions>(Path.Combine(FileUtils.ExecutingDir, CoreConstants.CONFIG_NAME_APP));
        }

        /***********************************************************************************/

        public string GetAppPath()
        {
            return Path.Combine(FileUtils.EntryDir, "dfn.exe");
        }

        public string GetInjectorDirectory()
        {
            var dir = Options.InjectorDirectory;
            if (string.IsNullOrEmpty(dir))
                dir = @"..\injector";
            return FileUtils.GetFullPath(dir);
        }

        public string GetTestRunnerDirectory()
        {
            var dir = Options.TestRunnerDirectory;
            if (string.IsNullOrEmpty(dir))
                dir = @"..\test_runner";
            return FileUtils.GetFullPath(dir);
        }

        public List<string> GetInjectorConfigs(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir))
                throw new Exception("The directory of Injector is empty in config");
            if (!Directory.Exists(dir))
                throw new Exception("The directory of Injector does not exist");
            //
            var allCfgs = Directory.GetFiles(dir, "*.yml");
            var res = new List<string>();
            foreach (var file in allCfgs)
            {
                try
                {
                    var cfg = ReadInjectorOptions(file);
                    if (cfg?.Type == CoreConstants.SUBSYSTEM_INJECTOR)
                        res.Add(FileUtils.GetFullPath(file, dir));
                }
                catch { }
            }
            return res;
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

        public InjectorOptions ReadInjectorOptions(string cfgPath)
        {
            return ReadOptions<InjectorOptions>(cfgPath);
        }

        public void WriteInjectorOptions(InjectorOptions opts, string cfgPath)
        {
            WriteOptions<InjectorOptions>(opts, cfgPath);
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
