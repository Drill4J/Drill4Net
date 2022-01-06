using System;
using System.IO;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;
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
        private readonly BaseOptionsHelper _optHelper;
        private readonly Logger _logger;

        /***********************************************************************************/

        public ConfiguratorRepository(): this(null) { }

        public ConfiguratorRepository(ConfiguratorOptions? opts) : base(CoreConstants.SUBSYSTEM_CONFIGURATOR, false)
        {
            _logger = new TypedLogger<ConfiguratorRepository>(Subsystem);
            Options = opts ?? ReadOptions<ConfiguratorOptions>(Path.Combine(FileUtils.ExecutingDir, CoreConstants.CONFIG_NAME_APP));
            _optHelper = new BaseOptionsHelper(Subsystem);
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

        public List<string> GetConfigs<T>(string subsystem, string dir) where T : AbstractOptions, new()
        {
            if (string.IsNullOrWhiteSpace(dir))
                throw new Exception($"The directory of {subsystem} is empty in config");
            if (!Directory.Exists(dir))
                throw new Exception($"The directory of {subsystem} does not exist");
            //
            var allCfgs = Directory.GetFiles(dir, "*.yml");
            var res = new List<string>();
            foreach (var file in allCfgs)
            {
                try
                {
                    var cfg = ReadOptions<T>(file);
                    if (cfg?.Type?.Equals(subsystem, StringComparison.InvariantCultureIgnoreCase) == true)
                        res.Add(FileUtils.GetFullPath(file, dir));
                }
                catch { }
            }
            return res;
        }

        #region Redirecting
        public string GetActualConfigPath(string dir)
        {
            return _optHelper.GetActualConfigPath(dir);
        }

        public string CalcRedirectConfigPath(string dir)
        {
            return _optHelper.CalcRedirectConfigPath(dir);
        }

        public RedirectData ReadRedirectData(string dir)
        {
            return _optHelper.ReadRedirectData(dir);
        }

        public void WriteRedirectData(RedirectData data, string dir)
        {
            _optHelper.WriteRedirectData(data, dir);
        }
        #endregion
        #region Read/write options
        public void SaveSystemConfiguration(SystemConfiguration cfg)
        {
            //Agent's options
            var agCfgPath = GetAgentConfigPath();
            var agentOpts = ReadAgentOptions(agCfgPath);
            agentOpts.Admin.Url = cfg.AdminUrl;
            agentOpts.PluginDir = cfg.AgentPluginDirectory;
            WriteAgentOptions(agentOpts, agCfgPath);

            // Transmitter opts
            var transCfgPath = GetTransmitterConfigPath();
            var transOpts = ReadMessagerOptions(transCfgPath);

            transOpts.Servers.Clear();
            transOpts.Servers.Add(cfg.MiddlewareUrl);

            WriteMessagerOptions(transOpts, transCfgPath);
        }

        public string GetTransmitterDir()
        {
            var transDir = Options.TransmitterDirectory;
            if (string.IsNullOrEmpty(transDir))
                transDir = @"..\transmitter";
            return transDir;
        }

        public string GetTransmitterConfigPath()
        {
            var transDir = GetTransmitterDir();
            return  Path.Combine(transDir, CoreConstants.CONFIG_NAME_MIDDLEWARE);
        }

        public string GetAgentConfigPath()
        {
            return Path.Combine(Options.InstallDirectory, CoreConstants.CONFIG_NAME_DEFAULT);
        }

        public string GetCiDirectory()
        {
            var transDir = Options.CiDirectory;
            if (string.IsNullOrEmpty(transDir))
                transDir = @"..\..\ci";
            return transDir;
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
        #endregion

        public string GetExternalEditor()
        {
            var path = Options.ExternalEditor;
            if (!string.IsNullOrWhiteSpace(path))
                return path;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "notepad.exe");
            }
            else //UNIX
            {
                //TODO: search for default editor (Vim, ee, nano, Kate, etc...)
                return string.Empty;
            }
        }
    }
}
