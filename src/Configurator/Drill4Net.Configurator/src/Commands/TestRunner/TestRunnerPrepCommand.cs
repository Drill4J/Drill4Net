using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER, ConfiguratorConstants.COMMAND_PREP)]
    public class TestRunnerPrepCommand : AbstractConfiguratorCommand
    {
        public TestRunnerPrepCommand(ConfiguratorRepository rep, CliCommandRepository cliRep): base(rep, cliRep)
        {
        }

        /**************************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            var force = IsSwitchSet(CoreConstants.SWITCH_FORCE);
            var injectorDir = _rep.GetInjectorDirectory();
            var destDir = GetParameter(CoreConstants.ARGUMENT_DESTINATION_DIR, false); //injected target dir
            if (!string.IsNullOrWhiteSpace(destDir)) //by injected target dir
            {
                return Task.FromResult((ProcessInjectedTarget(destDir, force), new Dictionary<string, object>()));
            }
            else //by switches (last, active configs...)
            {
                var runCfg = GetParameter(CoreConstants.ARGUMENT_RUN_CFG, false); //by TestRunner config (several targets)
                if (runCfg != null)
                {
                    if (!_cmdHelper.CheckConfigPath(CoreConstants.SUBSYSTEM_TEST_RUNNER, runCfg))
                        return Task.FromResult(FalseEmptyResult);
                    var runOpts = _rep.ReadTestRunnerOptions(runCfg);
                    var runnerDir = _rep.GetTestRunnerDirectory();
                    var res = true;
                    if (runOpts == null)
                    {
                        RaiseError("Run options is empty. Check type of the config.");
                        return Task.FromResult(FalseEmptyResult);
                    }
                    if (runOpts.Directories == null)
                    {
                        RaiseError("Directories are empty in the run options. Check the config.");
                        return Task.FromResult(FalseEmptyResult);
                    }
                    //
                    foreach (var runDirOpts in runOpts.Directories)
                    {
                        var trgDir = FileUtils.GetFullPath(runDirOpts.Directory, runnerDir);
                        if (!ProcessInjectedTarget(trgDir, false))
                            res = false;
                    }
                    return Task.FromResult(res ? TrueEmptyResult : FalseEmptyResult);
                }
                else //is the target specified?
                {
                    if (_desc == null)
                        return Task.FromResult(FalseEmptyResult);

                    var res = _cmdHelper.GetSourceConfigPath<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR,
                        injectorDir, _desc, out var cfgPath, out var _, out string? err);
                    if (!res)
                    {
                        if (string.IsNullOrWhiteSpace(err))
                            err = "You have to specify either the name of the config or the folder with the instrumented target.";
                        _logger.Error(err);
                        RaiseError(err);
                        return Task.FromResult(FalseEmptyResult);
                    }
                    return Task.FromResult((processConfig(cfgPath, force), new Dictionary<string, object>()));
                }
            }

            bool processConfig(string agentCfgPath, bool forceRewrite)
            {
                try
                {
                    var trgDir = _rep.GetTargetDestinationDir(agentCfgPath);
                    return ProcessInjectedTarget(trgDir, forceRewrite);
                }
                catch (Exception ex)
                {
                    RaiseError(ex.Message);
                    return false;
                }
            }
        }

        internal bool ProcessInjectedTarget(string trgDestDir, bool force)
        {
            string? err;
            if (!Directory.Exists(trgDestDir))
            {
                err = $"Target destination directory does not exist: [{trgDestDir}]. Perform the target injection first by specified {CoreConstants.SUBSYSTEM_INJECTOR} config.";
                _logger.Error(err);
                RaiseError(err);
                return false;
            }
            //
            var trgCfgPath = Path.Combine(trgDestDir, _rep.GetAgentTargetConfigName());
            var agCfgS = $"{CoreConstants.SUBSYSTEM_AGENT} config";
            if (force || !File.Exists(trgCfgPath))
            {
                var modelCfgPath = _rep.GetAgentModelConfigPath();
                if (!File.Exists(modelCfgPath))
                {
                    RaiseError($"Model {agCfgS} not found: [{modelCfgPath}]");
                    return false;
                }
                try //rewriting
                {
                    var opts = _rep.ReadAgentOptions(modelCfgPath);

                    //get full paths
                    var relDir = FileUtils.EntryDir;
                    opts.PluginDir = FileUtils.GetFullPath(opts.PluginDir, relDir);
                    opts.Connector.LogDir = FileUtils.GetFullPath(opts.Connector.LogDir, relDir);
                    
                    //save
                    _rep.WriteAgentOptions(opts, trgCfgPath);
                    RaiseMessage($"{agCfgS} is written to the target directory: [{trgCfgPath}]", CliMessageType.Info);
                }
                catch (Exception ex)
                {
                    err = $"Default {agCfgS} exists in the install directory but cannot be read: [{modelCfgPath}]";
                    _logger?.Error(err, ex);
                    RaiseError(err);
                }
            }
            else
            {
                try
                {
                    //check cfg itself
                    var opts = _rep.ReadAgentOptions(trgCfgPath);
                    //here we just check the fact of reading the config
                    RaiseMessage($"{agCfgS} is ok: [{trgCfgPath}]", CliMessageType.Info);
                }
                catch (Exception ex)
                {
                    err = $"{agCfgS} exist in target directory but cannot be read: [{trgCfgPath}]";
                    _logger?.Error(err, ex);
                    RaiseError(err);
                }
            }

            return true;
        }

        public override string GetShortDescription()
        {
            return "Prepare the injected target to run it if needed.";
        }

        public override string GetHelp()
        {
            //here are mentioned exactly INJECTOR configs!
            return @$"Check and prepare the injected target for additional requirements, it is now the presence of an {CoreConstants.SUBSYSTEM_AGENT} config in instrumented directory. If necessary, such a config is created using a model file, which, in turn, is configured by system settings. The Injector does it itself, but if the system settings were changed after that, you need to do the same manually and using the ""f"" switch (forced overwrite).

{HelpHelper.GetActiveLastSwitchesDesc(CoreConstants.SUBSYSTEM_INJECTOR, RawContexts)}
    Example: {RawContexts} -lf (forced)

{HelpHelper.GetPositionalConfigDesc(CoreConstants.SUBSYSTEM_INJECTOR, RawContexts, "injections")}

Another option is passing the injected target directory directly using ""{CoreConstants.ARGUMENT_DESTINATION_DIR}"" option:
    Example: {RawContexts} --{CoreConstants.ARGUMENT_DESTINATION_DIR}=""d:\Targets\TargetA.Injected\"" -f (forced)

You can even pass the {CoreConstants.SUBSYSTEM_TEST_RUNNER} config with several targets directories using ""{CoreConstants.ARGUMENT_RUN_CFG}"" option:
    Example: {RawContexts} --{CoreConstants.ARGUMENT_RUN_CFG}=""d:\test_runner\run1.yml""";
        }
    }
}
