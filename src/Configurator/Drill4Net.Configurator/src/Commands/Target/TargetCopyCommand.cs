using System;
using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Repository;

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
            var sourceName = string.Empty;

            //switches
            var copyActive = IsSwitchSet('a'); //copy active
            if (copyActive)
            {
                var actualCfg = new BaseOptionsHelper(_rep.Subsystem)
                    .GetActualConfigPath(dir);
                sourceName = Path.GetFileName(actualCfg);
            }
            if(sourceName == string.Empty)
            {
                var copyLast = IsSwitchSet('l');
                if (copyLast)
                {
                    var configs = _rep.GetInjectorConfigs(dir);
                    var lastEditedFile = string.Empty;
                    var dt = DateTime.MaxValue;
                    foreach (var config in configs)
                    {
                        var fdt = File.GetLastWriteTime(config);
                        if (fdt > dt)
                            continue;
                        dt = fdt;
                        lastEditedFile = config;
                    }
                    if(lastEditedFile != string.Empty)
                        sourceName = Path.GetFileName(lastEditedFile);
                }
            }

            //params
            var delta = sourceName == string.Empty ? 0 : 1;

            //source path
            if(string.IsNullOrWhiteSpace(sourceName))
                sourceName = GetPositional(0);
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                RaiseError("The source config is not specified, see help.");
                return Task.FromResult(false);
            }
            if (!string.IsNullOrWhiteSpace(Path.GetDirectoryName(sourceName)))
            {
                RaiseError("The source config should be just a file name without a directory, see help.");
                return Task.FromResult(false);
            }
            if (!sourceName.EndsWith(".yml"))
                sourceName += ".yml";
            var sourcePath = Path.Combine(dir, sourceName);
            if (!File.Exists(sourcePath))
            {
                RaiseError($"Source config not found: [{sourcePath}]");
                return Task.FromResult(false);
            }

            // dest path
            var destName = GetPositional(1 - delta);
            if (string.IsNullOrWhiteSpace(destName))
            {
                RaiseError("The destination config is not specified, see help.");
                return Task.FromResult(false);
            }
            if (!string.IsNullOrWhiteSpace(Path.GetDirectoryName(destName)))
            {
                RaiseError("The destination config should be just a file name without a directory.");
                return Task.FromResult(false);
            }
            if (!destName.EndsWith(".yml"))
                destName += ".yml";
            var destPath = Path.Combine(dir, destName);

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
                RaiseError($"Source section if config {sourceName} is empty.");
                return Task.FromResult(false);
            }
            if (targetVersion.EndsWith(".dll") || targetVersion.EndsWith(".exe"))
                cfg.Target.VersionAssemblyName = targetVersion;
            else
                cfg.Target.Version = targetVersion;

            var dest = cfg.Destination;
            if (dest == null)
            {
                RaiseError($"Destination section if config {sourceName} is empty.");
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
