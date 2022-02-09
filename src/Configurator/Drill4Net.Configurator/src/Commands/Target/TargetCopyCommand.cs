﻿using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         //ConfiguratorConstants.CONTEXT_CFG, 
                         ConfiguratorConstants.COMMAND_COPY)]
    public class TargetCopyCommand : AbstractConfiguratorCommand
    {
        public TargetCopyCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /***************************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return Task.FromResult(FalseEmptyResult);
            //
            var defDir = _rep.GetInjectorDirectory();

            // source path
            var res = _cmdHelper.GetSourceConfigPath<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR, defDir, _desc,
                out var sourcePath, out var fromSwitch, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(FalseEmptyResult);
            }

            var delta = fromSwitch ? 1 : 0;

            // dest path
            var destName = GetPositional(1 - delta) ?? "";
            res = _cmdHelper.GetConfigPath(defDir, "destination", destName, false, out var destPath, out error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(FalseEmptyResult);
            }

            //target version/assembly
            var targetVersion = GetPositional(2 - delta);
            if (string.IsNullOrWhiteSpace(targetVersion))
            {
                targetVersion = GetParameter(CoreConstants.ARGUMENT_TARGET_VERSION);
                if (!string.IsNullOrWhiteSpace(targetVersion))
                    delta++;
            }
            if (string.IsNullOrWhiteSpace(targetVersion))
            {
                RaiseError("Target's version/assembly is not specified.");
                return Task.FromResult(FalseEmptyResult);
            }

            //target working dir
            var targetDir = GetPositional(3 - delta); //empty value means the same directory
            if (string.IsNullOrWhiteSpace(targetDir))
                targetDir = GetParameter(CoreConstants.ARGUMENT_DESTINATION_DIR);

            //set config
            var cfg = _rep.ReadInjectorOptions(sourcePath);
            if (cfg.Target == null)
            {
                RaiseError($"Source section in config is empty: [{sourcePath}]");
                return Task.FromResult(FalseEmptyResult);
            }

            if (targetVersion.EndsWith(".dll") || targetVersion.EndsWith(".exe"))
                cfg.Target.VersionAssemblyName = targetVersion;
            else
                cfg.Target.Version = targetVersion;

            var dest = cfg.Destination;
            if (dest == null)
            {
                RaiseError($"Destination section in config is empty: [{sourcePath}]");
                return Task.FromResult(FalseEmptyResult);
            }
            if (!string.IsNullOrWhiteSpace(targetDir))
            {
                dest.Directory = targetDir;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dest.FolderPostfix))
                    dest.FolderPostfix = "Injected";
            }

            //save config
            res = _cmdHelper.SaveConfig(CoreConstants.SUBSYSTEM_INJECTOR, cfg, destPath);
            return Task.FromResult((res, new Dictionary<string, object>()));
        }

        public override string GetShortDescription()
        {
            return $"Copy the specified {CoreConstants.SUBSYSTEM_INJECTOR} config to new one with new name, version and optional injected target's directory.";
        }

        public override string GetHelp()
        {
            return @$"You should specifiy the target's version and destination directory for instrumented build. {HelpHelper.GetCopyConfigDesc(CoreConstants.SUBSYSTEM_INJECTOR, RawContexts, "targetA", @$"--{CoreConstants.ARGUMENT_TARGET_VERSION}=0.1.0 {CoreConstants.ARGUMENT_DESTINATION_DIR}=""C:\targetA_2""")}

Both the folder of the instrumented target and its version number can be specified just as positional parameters:
  Example: {RawContexts} ""d:\configs\targetA\source.yml"" ""d:\Drill4Net\targetA\destination.yml"" 0.1.0 ""C:\targetA_2""
  Example: {RawContexts} ""d:\configs\targetA\source.yml"" ""d:\Drill4Net\targetA\destination.yml"" 0.1.0

In the latter case, the destination directory will be generated by a source one plus the postfix specified in the initial configuration.";
        }
    }
}
