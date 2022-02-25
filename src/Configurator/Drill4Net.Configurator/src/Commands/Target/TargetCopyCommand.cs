using System.Threading.Tasks;
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

            //trg copy -- "d:\Projects\source.yml" "d:\Projects\dest.yml" --version=0.3.4 "c:\Downloads\123"
            //trg copy -- "d:\Projects\inj_razor.yml" "d:\Projects\dest.yml" --version=0.1.2 --postfix ABC
            //trg copy --version=0.1.2 "d:\Projects\dest.yml" -a
            //trg copy -- "d:\Projects\dest.yml" -a 0.1.2

            // source cfg path
            var res = _cmdHelper.GetSourceConfigPath<InjectionOptions>(CoreConstants.SUBSYSTEM_INJECTOR, defDir, _desc,
                out var sourcePath, out var fromPos, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(FalseEmptyResult);
            }

            var pos = fromPos ? 1 : 0;

            // dest cfg path
            var destCfg = GetParameter(CoreConstants.ARGUMENT_DESTINATION_PATH);
            if (string.IsNullOrWhiteSpace(destCfg))
            {
                destCfg = GetPositional(pos) ?? "";
                if (!string.IsNullOrWhiteSpace(destCfg))
                    pos++;
            }
            res = _cmdHelper.GetConfigPath(defDir, "destination", destCfg, false, out var destPath, out error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(FalseEmptyResult);
            }

            //target version/assembly
            var targetVersion = GetParameter(CoreConstants.ARGUMENT_TARGET_VERSION);
            if (string.IsNullOrWhiteSpace(targetVersion))
            {
                targetVersion = GetPositional(pos);
                if (!string.IsNullOrWhiteSpace(targetVersion))
                    pos++;
            }
            if (string.IsNullOrWhiteSpace(targetVersion))
            {
                RaiseError("Target's version/assembly is not specified.");
                return Task.FromResult(FalseEmptyResult);
            }

            //target destination working dir (empty value means the SAME directory)
            var targetDir = GetParameter(CoreConstants.ARGUMENT_DESTINATION_DIR);
            if (string.IsNullOrWhiteSpace(targetDir))
                targetDir = GetPositional(pos);

            //set up the config
            var cfg = _rep.ReadInjectionOptions(sourcePath);
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
            
            //postfix (if the dir is empty, including in the future)
            var postfix = GetParameter(CoreConstants.ARGUMENT_POSTFIX);
            if (string.IsNullOrWhiteSpace(postfix))
            {
                if (string.IsNullOrWhiteSpace(dest.FolderPostfix))
                    dest.FolderPostfix = CoreConstants.INJECTION_DESTITANTION_POSTFIX;
            }
            else
            {
                dest.FolderPostfix = postfix;
                dest.Directory = null;
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
            return @$"You should specifiy the target's version and destination directory for instrumented build. {HelpHelper.GetCopyConfigDesc(CoreConstants.SUBSYSTEM_INJECTOR, RawContexts, "targetA", @$"--{CoreConstants.ARGUMENT_TARGET_VERSION}=0.1.0 --{CoreConstants.ARGUMENT_DESTINATION_DIR}=""C:\instrumented-dir""")}

Directory of source and instrumented targets and version number of the last one can be specified just as positional parameters:
  Example: {RawContexts} -- ""d:\configs\targetA\source.yml"" ""d:\Drill4Net\targetA\destination.yml"" 0.1.0 ""C:\instrumented-dir""
  Example: {RawContexts} -- ""d:\configs\targetA\source.yml"" ""d:\Drill4Net\targetA\destination.yml"" 0.1.0

In the latter case, the destination directory will be generated by a source one plus the postfix specified in the initial configuration ('Injected' by default), but you can specify it, too (only by the named argument):
  Example: {RawContexts} -- ""d:\configs\targetA\source.yml"" ""d:\Drill4Net\targetA\destination.yml"" 0.1.0 --{CoreConstants.ARGUMENT_POSTFIX}=Instrumented

Position order: source's config path, destination's config path, destination's version, target's output directory. Positions are used on a residual basis if the corresponding named argument is not found.
The target output directory is not mandatory, the empty value means the SAME directory as in source config (its value will be copied). To use its automatic generation, specify the prefix, but if both ones are specified, the concrete instrumented directory will be used.";
        }
    }
}
