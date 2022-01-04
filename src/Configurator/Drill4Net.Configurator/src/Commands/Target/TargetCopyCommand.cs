using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET, ConfiguratorConstants.COMMAND_COPY)]
    public class TargetCopyCommand : AbstractConfiguratorCommand
    {
        public TargetCopyCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /***************************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetInjectorDirectory();

            // source path
            var res = _commandHelper.GetSourceConfig(dir, this, out var sourcePath, out var fromSwitch, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }

            var delta = fromSwitch ? 1 : 0;

            // dest path
            var destName = GetPositional(1 - delta);
            res = _commandHelper.GetConfigPath(dir, "destination", destName, false, out var destPath, out error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }

            //target version/assembly
            var targetVersion = GetPositional(2 - delta);
            if (string.IsNullOrWhiteSpace(targetVersion))
            {
                RaiseError("Target's version/assembly is not specified.");
                return Task.FromResult(false);
            }

            //target working dir
            var targetDir = GetPositional(3 - delta); //empty value means the same directory

            //set config
            var cfg = _rep.ReadInjectorOptions(sourcePath);
            if (cfg.Target == null)
            {
                RaiseError($"Source section in config is empty: [{sourcePath}]");
                return Task.FromResult(false);
            }
            if (targetVersion.EndsWith(".dll") || targetVersion.EndsWith(".exe"))
                cfg.Target.VersionAssemblyName = targetVersion;
            else
                cfg.Target.Version = targetVersion;

            var dest = cfg.Destination;
            if (dest == null)
            {
                RaiseError($"Destination section in config is empty: [{sourcePath}]");
                return Task.FromResult(false);
            }
            if (!string.IsNullOrWhiteSpace(targetDir))
            {
                dest.Directory = targetDir;
            }
            else
                if (string.IsNullOrWhiteSpace(dest.FolderPostfix))
                    dest.FolderPostfix = "Injected";

            //save config
            _rep.WriteInjectorOptions(cfg, destPath);
            RaiseMessage($"Config saved to [{destPath}]", CliMessageType.Info);
            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return "Copy the specified Injector's config to new one with new name, version and optional injected target's directory";
        }

        public override string GetHelp()
        {
            return "Help article not implemeted yet";
        }
    }
}
