using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_CHECK)]
    public class TargetCheckCommand : AbstractConfiguratorCommand
    {
        public TargetCheckCommand(ConfiguratorRepository rep, CliCommandRepository cliRep): base(rep, cliRep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
        {
            if (_desc == null)
                return Task.FromResult(false);

            // open cfg
            var dir = _rep.GetInjectorDirectory();
            var res = _cmdHelper.GetSourceConfigPath<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR,
                dir, _desc, out var cfgPath, out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }
            RaiseMessage($"\nChecking: [{cfgPath}]", CliMessageType.Info);
            //
            var opts = _rep.ReadInjectorOptions(cfgPath, true);
            var destDir = opts.Destination.Directory ?? "";
            string check;

            //target
            var target = opts.Target;
            _cmdHelper.WriteCheck("Target name", "Name is empty", !string.IsNullOrWhiteSpace(target.Name));
            if (!string.IsNullOrWhiteSpace(target.Version))
            {
                _cmdHelper.WriteCheck("Target version", "", true);
            }
            else
            {
                var asmName = target.VersionAssemblyName;
                if (!string.IsNullOrWhiteSpace(asmName))
                {
                    check = "Target version assembly";
                    var asmPath = Path.Combine(destDir, asmName);
                    _cmdHelper.WriteCheck(check, $"The assembly for determining the target version was not found: [{asmPath}]", File.Exists(asmPath));
                }
                else
                {
                    _cmdHelper.WriteCheck("Target versioning", "", false);
                }
            }

            //source
            var source = opts.Source;

            check = "Source directory";
            var sourceDir = source?.Directory;
            if (string.IsNullOrWhiteSpace(sourceDir))
                _cmdHelper.WriteCheck(check, "Source directory path is empty", false);
            else
                _cmdHelper.WriteCheck(check, $"Source directory does not exist: [{sourceDir}]", Directory.Exists(sourceDir));

            //filter
            res = IsFilterPartOk(source?.Filter?.Includes) || IsFilterPartOk(source?.Filter?.Excludes);
            _cmdHelper.WriteCheck("Filter for injected entities", "No filter entry", res);

            //destination
            check = "Destination directory";
            if (string.IsNullOrWhiteSpace(destDir))
                _cmdHelper.WriteCheck(check, "Destination directory path is empty", false);
            else
                _cmdHelper.WriteCheck(check, "Destination directory does not exist", Directory.Exists(destDir));

            // proxy
            var proxy = opts.Proxy;
            _cmdHelper.WriteCheck("Proxy class", "Class name is empty", !string.IsNullOrWhiteSpace(proxy?.Class));
            _cmdHelper.WriteCheck("Proxy method", "Method name is empty", !string.IsNullOrWhiteSpace(proxy?.Method));

            //profiler (transmiter)
            var profiler = opts.Profiler;
            check = "Profiler directory";
            var profDir = profiler?.Directory;
            if (string.IsNullOrWhiteSpace(profDir))
                _cmdHelper.WriteCheck(check, "Directory path is empty", false);
            else
                _cmdHelper.WriteCheck(check, $"Directory does not exist: [{profDir}]", Directory.Exists(profDir));

            var profAsmName = profiler?.AssemblyName;
            _cmdHelper.WriteCheck("Profiler assembly name", "Assembly name is empty", !string.IsNullOrWhiteSpace(profAsmName));

            var profAsmPath = Path.Combine(profDir, profAsmName);
            _cmdHelper.WriteCheck("Profiler assembly file", "Assembly file is not found", File.Exists(profAsmPath));

            _cmdHelper.WriteCheck("Profiler namespace", "Namespace is empty", !string.IsNullOrWhiteSpace(profiler?.Namespace));
            _cmdHelper.WriteCheck("Profiler class", "Class name is empty", !string.IsNullOrWhiteSpace(profiler?.Class));
            _cmdHelper.WriteCheck("Profiler method", "Method name is empty", !string.IsNullOrWhiteSpace(profiler?.Method));

            //TODO: real check by Reflection

            // plugins
            check = "Contexter plugins";
            _cmdHelper.WriteCheck(check, "Plugin configuration error", CheckPlugins(check, opts.Plugins));

            // versions
            if (opts.Versions != null)
            {
                check = "Version section";
                _cmdHelper.WriteCheck(check, "Version configuration error", _cmdHelper.CheckVersions(check, sourceDir ?? "", opts.Versions));
            }
            //
            return Task.FromResult(true);
        }

        private bool CheckPlugins(string check, Dictionary<string, PluginLoaderOptions>? plugins)
        {
            // if there are no plugins, it can be ... for very simple targets
            if (plugins == null)
                return true;
            //
            var res = true;
            var injDir = _rep.GetInjectorDirectory();
            foreach (var name in plugins.Keys)
            {
                var plug = plugins[name];
                var dir = FileUtils.GetFullPath(plug.Directory, injDir);
                if(!Directory.Exists(dir))
                {
                    _cmdHelper.WriteCheck(check, $"Plugin {name}: the directory does not exist: [{dir}]", false);
                    res = false;
                }
                //
                var cfg = plug.Config;
                if (!cfg.EndsWith(".yml"))
                    cfg += ".yml";
                var cfgPath = FileUtils.GetFullPath(Path.Combine(injDir, cfg), dir);
                if (!File.Exists(cfgPath))
                {
                    _cmdHelper.WriteCheck(check, $"The specified config does not exist for plugin {name}: [{cfgPath}]", false);
                    res = false;
                }
            }
            return res;
        }

        private bool IsFilterPartOk(SourceFilterParams? pars)
        {
            if(pars == null)
                return false;
            return pars.Attributes?.Any() == true ||
                   pars.Classes?.Any() == true ||
                   pars.Directories?.Any() == true ||
                   pars.Files?.Any() == true ||
                   pars.Folders?.Any() == true ||
                   pars.Namespaces?.Any() == true;
        }

        public override string GetShortDescription()
        {
            return $"Checks the specified {CoreConstants.SUBSYSTEM_INJECTOR}'s configuration before target proccessing.";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}
