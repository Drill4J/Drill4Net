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
        public TestRunnerPrepCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
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

        internal bool ProcessInjectedTarget(string dir, bool force)
        {
            string? err;
            if (!Directory.Exists(dir))
            {
                err = $"Directory does not exist: [{dir}]. Perform the target injection first by specified {CoreConstants.SUBSYSTEM_INJECTOR} config.";
                _logger.Error(err);
                RaiseError(err);
                return false;
            }
            //
            var trCfgPath = Path.Combine(dir, _rep.GetAgentTargetConfigName());
            var agCfgS = $"{CoreConstants.SUBSYSTEM_AGENT} config";
            if (force || !File.Exists(trCfgPath))
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
                    var relDir = _rep.GetInstallDirectory();
                    opts.PluginDir = FileUtils.GetFullPath(opts.PluginDir, relDir);
                    opts.Connector.LogDir = FileUtils.GetFullPath(opts.Connector.LogDir, relDir);
                    
                    //save
                    _rep.WriteAgentOptions(opts, trCfgPath);
                    RaiseMessage($"{agCfgS} is written to the target directory: [{trCfgPath}]", CliMessageType.Info);
                }
                catch (Exception ex)
                {
                    err = $"Default {agCfgS} exists in install directory but cannot be read: [{modelCfgPath}]";
                    _logger?.Error(err, ex);
                    RaiseError(err);
                }
            }
            else
            {
                try
                {
                    //check cfg itself
                    var opts = _rep.ReadAgentOptions(trCfgPath);
                    //here we just check the fact of reading the config
                    RaiseMessage($"{agCfgS} is ok: [{trCfgPath}]", CliMessageType.Info);
                }
                catch (Exception ex)
                {
                    err = $"{agCfgS} exist in target directory but cannot be read: [{trCfgPath}]";
                    _logger?.Error(err, ex);
                    RaiseError(err);
                }
            }

            return true;
        }

        public override string GetShortDescription()
        {
            return "Prepare the injected target to run it.";
        }

        public override string GetHelp()
        {
            return @$"Check and prepare the injected target for additional requirements, it is now the presence of an {CoreConstants.SUBSYSTEM_AGENT} config in instrumented directory. If necessary, such a config is created using a model file, which, in turn, is configured by system settings. The Injector does it itself, but if the system settings were changed after that, you need to do the same manually and using the ""f"" switch (forced overwrite).

You can use some swithes for implicit specifying the {CoreConstants.SUBSYSTEM_INJECTOR} config which describes a specific injection: ""a"" for the active one and ""l"" for the last edited one.
    Example: run prep -a
    Example: run prep -l
    Example: run prep -lf (forced)

Also you can to do it by passing the explicit short name of {CoreConstants.SUBSYSTEM_INJECTOR} config file or its full path as positional parameter:
    Example: run prep -- cfg2
    Example: run prep -- ""d:\configs\injections\cfg2.yml""

...or with named argument:
    Example: run prep --cfg_path=""d:\configs\injections\cfg2.yml""

Another option is passing the injected target directory using ""dest_dir"" option:
    Example: run prep --dest_dir=""d:\Targets\TargetA.Injected\"" -f (forced)";
        }
    }
}
