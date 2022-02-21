using System;
using System.IO;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Repository;
using Drill4Net.Configuration;
using Drill4Net.Injector.Core;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.TestRunner.Core;
using Drill4Net.Injector.Engine;

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
            Options = opts ?? ReadConfiguratorOptions();
            _optHelper = new BaseOptionsHelper(Subsystem);
        }

        /***********************************************************************************/

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

        #region Dirs/paths
        public string GetAppPath()
        {
            return Path.Combine(FileUtils.EntryDir, "dfn.exe");
        }

        public string GetInstallDirectory()
        {
            var dir = Options.InstallDirectory;
            if (string.IsNullOrEmpty(dir))
                dir = ConfiguratorConstants.PATH_INSTALL;
            return dir;
        }

        public string GetInjectorDirectory()
        {
            var dir = Options.InjectorDirectory;
            if (string.IsNullOrEmpty(dir))
                dir = ConfiguratorConstants.PATH_INJECTOR;
            return FileUtils.GetFullPath(dir);
        }

        public string GetInjectorPath()
        {
            return Path.Combine(GetInjectorDirectory(), GetExecutableName("Drill4Net.Injector.App"));
        }

        public string GetTestRunnerDirectory()
        {
            var dir = Options.TestRunnerDirectory;
            if (string.IsNullOrEmpty(dir))
                dir = ConfiguratorConstants.PATH_RUNNER;
            return FileUtils.GetFullPath(dir);
        }

        public string GetTestRunnerPath()
        {
            return Path.Combine(GetTestRunnerDirectory(), GetExecutableName("Drill4Net.Agent.TestRunner"));
        }

        internal string GetExecutableName(string appFullName)
        {
            if(string.IsNullOrWhiteSpace(appFullName))
                throw new ArgumentNullException(nameof(appFullName));
            if (!appFullName.EndsWith("."))
                appFullName += ".";
            appFullName += CommonUtils.IsWindows() ? "exe" : "dll";
            return appFullName;
        }

        /// <summary>
        /// Get the Injector config as default model's one.
        /// </summary>
        /// <returns></returns>
        public string GetInjectorModelConfigPath()
        {
            return Path.Combine(GetInstallDirectory(), ConfiguratorConstants.CONFIG_INJECTOR_MODEL);
        }

        /// <summary>
        /// Get the Test Runner config as default model's one.
        /// </summary>
        /// <returns></returns>
        public string GetTestRunnerModelConfigPath()
        {
            return Path.Combine(GetInstallDirectory(), ConfiguratorConstants.CONFIG_TEST_RUNNER_MODEL);
        }

        /// <summary>
        /// Get the agent config as default model's one. It is also copied
        /// to the injected target directory for normal workflow.
        /// </summary>
        /// <returns></returns>
        public string GetAgentModelConfigPath()
        {
            return Path.Combine(GetInstallDirectory(), ConfiguratorConstants.CONFIG_AGENT_MODEL);
        }

        /// <summary>
        /// Get the agent config name for the injected Target.
        /// </summary>
        /// <returns></returns>
        public string GetAgentTargetConfigName()
        {
            return CoreConstants.CONFIG_NAME_DEFAULT;
        }

        public string GetTransmitterDir()
        {
            var transDir = Options.TransmitterDirectory;
            if (string.IsNullOrEmpty(transDir))
                transDir = ConfiguratorConstants.PATH_TRANSMITTER;
            return transDir;
        }

        /// <summary>
        /// Get the Transmitter config path (it connects to the middleware as Kafka,
        /// not Drill admin side)
        /// </summary>
        /// <returns></returns>
        public string GetTransmitterConfigPath()
        {
            var transDir = GetTransmitterDir();
            return Path.Combine(transDir, CoreConstants.CONFIG_NAME_MIDDLEWARE);
        }

        public string GetCiDirectory()
        {
            var transDir = Options.CiDirectory;
            if (string.IsNullOrEmpty(transDir))
                transDir = ConfiguratorConstants.PATH_CI;
            return transDir;
        }

        public string GetAgentPluginDirectory()
        {
            var dir = Options.AgentPluginDirectory;
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                dir = GetAppPath();
            return FileUtils.GetFullPath(dir);
        }

        public string GetInjectorPluginDirectory()
        {
            var dir = InjectorRepository.GetInjectorAppOptionsPath();
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                dir = GetAppPath();
            return FileUtils.GetFullPath(dir);
        }

        public string GetExternalEditor()
        {
            var path = Options.ExternalEditor;
            if (!string.IsNullOrWhiteSpace(path))
                return path;
            if (CommonUtils.IsWindows())
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "notepad.exe");
            }
            else //UNIX
            {
                //TODO: search for default editor (Vim, ee, nano, Kate, etc...)
                return string.Empty;
            }
        }
        #endregion
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
        public ConfiguratorOptions ReadConfiguratorOptions()
        {
            return ReadOptions<ConfiguratorOptions>(GetConfiguratorConfigPath());
        }

        public void WriteConfiguratorOptions(ConfiguratorOptions opts)
        {
            WriteOptions<ConfiguratorOptions>(opts, GetConfiguratorConfigPath());
        }

        internal string GetConfiguratorConfigPath()
        {
            return Path.Combine(FileUtils.ExecutingDir, CoreConstants.CONFIG_NAME_APP);
        }

        public void SaveSystemConfiguration(SystemConfiguration cfg)
        {
            //Agent's options - for Admin side, using the Agent Worker
            var agCfgPath = GetAgentModelConfigPath();
            var agentOpts = ReadAgentOptions(agCfgPath);
            agentOpts.Admin.Url = cfg.AdminUrl;
            agentOpts.PluginDir = cfg.AgentPluginDirectory; //IGeneratorContexter

            WriteAgentOptions(agentOpts, agCfgPath);

            //Transmitter's opts - middleware (as Kafka) using inside Target
            var transCfgPath = GetTransmitterConfigPath();
            var transOpts = ReadMessagerOptions(transCfgPath);

            transOpts.Servers.Clear();
            transOpts.Servers.Add(cfg.MiddlewareUrl);

            //save options for the Injector app
            var injOpts = ReadInjectorAppOptions();
            injOpts.PluginDir = cfg.InjectorPluginDirectory; //IInjectorPlugin
            WriteInjectorAppOptions(injOpts);

            //writing config to native Transmitter dir
            WriteMessagerOptions(transOpts, transCfgPath);
        }

        public void SetDefaultSystemConfiguration()
        {
            var defCfg = new SystemConfiguration
            {
                AdminUrl = "localhost:8090",
                MiddlewareUrl = "localhost:9093",
            };
            SaveSystemConfiguration(defCfg);
        }

        public InjectorAppOptions ReadInjectorAppOptions()
        {
            return InjectorRepository.GetInjectorAppOptions();
        }

        public void WriteInjectorAppOptions(InjectorAppOptions opts)
        {
            WriteOptions<InjectorAppOptions>(opts, InjectorRepository.GetInjectorAppOptionsPath());
        }

        public InjectionOptions ReadInjectionOptions(string cfgPath, bool processed = false)
        {
            var optHelper = processed ?
                new InjectionOptionsHelper() :
                new BaseOptionsHelper<InjectionOptions>(Subsystem);
            return optHelper.ReadOptions(cfgPath);
        }

        public void WriteInjectionOptions(InjectionOptions opts, string cfgPath)
        {
            var optHelper = new InjectionOptionsHelper();
            optHelper.WriteOptions(opts, cfgPath);
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
            if (validate)
            {
                var cfgDirs = opts.Injection?.ConfigDir;
                if (string.IsNullOrWhiteSpace(cfgDirs))
                    throw new ArgumentException($"The directory path for {CoreConstants.SUBSYSTEM_INJECTOR} configs is empty");
                cfgDirs = FileUtils.GetFullPath(cfgDirs);
                if (!Directory.Exists(cfgDirs))
                    throw new ArgumentException($"The directory for {CoreConstants.SUBSYSTEM_INJECTOR} configs does not exist. Any file path must be in quotes.");

                var runnerCfg = opts.TestRunnerConfigPath;
                if (string.IsNullOrWhiteSpace(runnerCfg))
                    throw new ArgumentException($"The {CoreConstants.SUBSYSTEM_TEST_RUNNER} config path is empty");
                runnerCfg = FileUtils.GetFullPath(runnerCfg);
                if (!File.Exists(runnerCfg))
                    throw new ArgumentException($"The {CoreConstants.SUBSYSTEM_TEST_RUNNER} config does not exist");
            }
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
            cfgPath = FileUtils.GetFullPath(cfgPath);
            if (!File.Exists(cfgPath))
                throw new FileNotFoundException($"File not found. Note: any path must be in quotes. Path: [{cfgPath}]");
            var optHelper = new BaseOptionsHelper<T>(Subsystem);
            return optHelper.ReadOptions(cfgPath);
        }

        public void WriteOptions<T>(T opts, string cfgPath) where T : AbstractOptions, new()
        {
            var optHelper = new BaseOptionsHelper<T>(Subsystem);
            optHelper.WriteOptions(opts, cfgPath);
        }
        #endregion

        public string GetTargetDestinationDir(string cfgPath)
        {
            string? err;
            if (!File.Exists(cfgPath))
            {
                err = $"{CoreConstants.SUBSYSTEM_INJECTOR} config does not exist: [{cfgPath}]";
                _logger.Error(err);
                throw new Exception(err);
            }
            try
            {
                var opts = ReadInjectionOptions(cfgPath, true); //it needs to be processed to get the destination path
                return opts.Destination.Directory;
            }
            catch (Exception ex)
            {
                err = $"The {CoreConstants.SUBSYSTEM_INJECTOR} config cannot be read: [{cfgPath}]";
                _logger.Error(err, ex);
                throw new Exception(err);
            }
        }
    }
}
