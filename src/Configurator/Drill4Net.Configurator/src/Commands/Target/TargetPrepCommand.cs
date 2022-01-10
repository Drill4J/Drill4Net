using System;
using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET, ConfiguratorConstants.COMMAND_PREP)]
    public class TargetPrepCommand : AbstractConfiguratorCommand
    {
        public TargetPrepCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
        {
            var force = IsSwitchSet(CoreConstants.SWITCH_FORCE);
            var injCfg = GetPositional(0); //cfg name
            var injDir = GetParameter(CoreConstants.ARGUMENT_DESTINATION_DIR, false); //injected target dir
            if (!string.IsNullOrWhiteSpace(injCfg)) //by config
            {
                if (!Path.HasExtension(injCfg))
                    injCfg += ".yml";

                var cfgPath = string.IsNullOrWhiteSpace(Path.GetPathRoot(injCfg)) ? //is it full path?
                    Path.Combine(_rep.GetInjectorDirectory(), injCfg) : //local config for the Injector
                    injCfg;

                return Task.FromResult(processConfig(cfgPath, force));
            }
            else
            if (!string.IsNullOrWhiteSpace(injDir)) //by injected target dir
            {
                return Task.FromResult(ProcessInjectedTarget(injDir, force));
            }
            else //by switches (last, active configs...)
            {
                if (_desc == null)
                    return Task.FromResult(false);

                var dir = _rep.GetInjectorDirectory();
                var res = _cmdHelper.GetSourceConfigPath<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR, dir, _desc, out var cfgPath,
                    out var _, out string? err);
                if (!res)
                {
                    if (string.IsNullOrWhiteSpace(err))
                        err = "You have to specify either the name of the config or the folder with the instrumented target.";
                    _logger.Error(err);
                    RaiseError(err);
                    return Task.FromResult(false);
                }
                return Task.FromResult(processConfig(cfgPath, force));
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
                err = $"Directory does not exist: [{dir}]. Perform the target injection first by specified {CoreConstants.SUBSYSTEM_INJECTOR}'s config.";
                _logger.Error(err);
                RaiseError(err);
                return false;
            }
            //
            var trCfgPath = Path.Combine(dir, _rep.GetAgentTargetConfigName());
            var agCfgS = $"{CoreConstants.SUBSYSTEM_AGENT}'s config";
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
            return @$"Check and prepare the injected target for additional requirements, it is now the presence of an {CoreConstants.SUBSYSTEM_AGENT}'s config in instrumented directory. If necessary, such a config is created using a model file, which, in turn, is configured by system settings. The Injector does it itself, but if the system settings were changed after that, you need to do the same manually and using the ""f"" switch (forced overwrite).

You can use some swithes for implicit specifying the {CoreConstants.SUBSYSTEM_INJECTOR}'s config which describes a specific injection: ""a"" for the active one and ""l"" for the last edited one.
    Example: trg prep -a
    Example: trg prep -l
    Example: trg prep -lf (forced)

Also you can to do it by passing the explicit short name of {CoreConstants.SUBSYSTEM_INJECTOR}'s config file or its full path as positional parameter:
    Example: trg prep -- cfg2
    Example: trg prep -- ""d:\Targets\TargA.Injected\cfg2.yml""

Another option is passing the injected target directory using ""dest_dir"" option:
    Example: trg prep --dest_dir=""d:\Targets\TargA.Injected\"" -f (forced)";
        }
    }
}
