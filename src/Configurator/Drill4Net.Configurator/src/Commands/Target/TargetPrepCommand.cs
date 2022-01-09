using System;
using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

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
            var injCfg = GetPositional(0); //cfg name
            var injDir = GetParameter(CoreConstants.ARGUMENT_DESTINATION_DIR, false); //injected target dir
            string? err;
            if (!string.IsNullOrWhiteSpace(injCfg))
            {
                if (!Path.HasExtension(injCfg))
                    injCfg += ".yml";

                var cfgPath = string.IsNullOrWhiteSpace(Path.GetPathRoot(injCfg)) ? //is it full path?
                    Path.Combine(_rep.GetInjectorDirectory(), injCfg) : //local config for the Injector
                    injCfg;

                if(!File.Exists(cfgPath))
                {
                    err = $"{CoreConstants.SUBSYSTEM_INJECTOR}'s config does not exist: [{cfgPath}]";
                    _logger.Error(err);
                    RaiseError(err);
                    return Task.FromResult(false);
                }
                try
                {
                    var opts = _rep.ReadInjectorOptions(cfgPath, true); //it needs to be processed to get the destination path
                    return Task.FromResult(CheckInjectedTarget(opts.Destination.Directory));
                }
                catch(Exception ex)
                {
                    err = $"The {CoreConstants.SUBSYSTEM_INJECTOR}'s config cannot be read: [{cfgPath}]";
                    _logger.Error(err, ex);
                    RaiseError(err);
                    return Task.FromResult(false);
                }
            }
            else
            if (!string.IsNullOrWhiteSpace(injDir))
            {
                return Task.FromResult(CheckInjectedTarget(injDir));
            }
            else
            {
                err = "You have to specify either the name of the config or the folder with the instrumented target.";
                _logger.Error(err);
                RaiseError(err);
                return Task.FromResult(false);
            }
        }

        internal bool CheckInjectedTarget(string dir)
        {
            if (!Directory.Exists(dir))
            {
                var err = $"Directory does not exist: [{dir}]. Perform the target injection first by specified {CoreConstants.SUBSYSTEM_INJECTOR}'s config.";
                _logger.Error(err);
                RaiseError(err);
                return false;
            }

            return true;
        }

        public override string GetShortDescription()
        {
            return "<implementation in process...>";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}
