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
            var globRes = true;

            //open cfg
            var cfgPath = GetParameter(CoreConstants.ARGUMENT_CONFIG_PATH);
            if (string.IsNullOrWhiteSpace(cfgPath))
            {
                if (_desc == null)
                    return Task.FromResult(false);
                var dir = _rep.GetInjectorDirectory();
                var res2 = _cmdHelper.GetSourceConfigPath<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR,
                    dir, _desc, out cfgPath, out var _, out var error);
                if (!res2)
                {
                    RaiseError(error);
                    return Task.FromResult(false);
                }
            }
            else
            {
                if (!File.Exists(cfgPath))
                {
                    RaiseError($"Specified by parameter config is not found: [{cfgPath}]");
                    return Task.FromResult(false);
                }
            }

            RaiseMessage($"\nChecking: [{cfgPath}]", CliMessageType.Info);
            //
            var opts = _rep.ReadInjectorOptions(cfgPath ?? "", true);
            var destDir = opts.Destination.Directory ?? "";
            string check;

            //target
            var target = opts.Target;
            _cmdHelper.RegCheck("Target name", "Name is empty", !string.IsNullOrWhiteSpace(target.Name), ref globRes);

            //an empty and direct version, and a "assembly's version" is normal (but is not best) -
            //the system will try to get the version at runtime "from somewhere"
            if (!string.IsNullOrWhiteSpace(target.Version))
            {
                _cmdHelper.RegCheck("Target version", "", true, ref globRes);
            }
            else
            {
                var asmName = target.VersionAssemblyName;
                if (!string.IsNullOrWhiteSpace(asmName))
                {
                    check = "Target version assembly";
                    var asmPath = Path.Combine(destDir, asmName);
                    _cmdHelper.RegCheck(check, $"The assembly for determining the target version was not found: [{asmPath}]",
                        File.Exists(asmPath), ref globRes);
                }
            }

            //source
            var source = opts.Source;

            check = "Source directory";
            var sourceDir = source?.Directory;
            if (string.IsNullOrWhiteSpace(sourceDir))
                _cmdHelper.RegCheck(check, "Source directory path is empty", false, ref globRes);
            else
                _cmdHelper.RegCheck(check, $"Source directory does not exist: [{sourceDir}]",
                    Directory.Exists(sourceDir), ref globRes);

            //filter
            var res = IsFilterPartOk(source?.Filter?.Includes) || IsFilterPartOk(source?.Filter?.Excludes);
            _cmdHelper.RegCheck("Filter for injected entities", "No filter entry", res, ref globRes);

            //destination
            check = "Destination directory";
            if (string.IsNullOrWhiteSpace(destDir))
                _cmdHelper.RegCheck(check, "Destination directory path is empty", false, ref globRes);
            else
                _cmdHelper.RegCheck(check, "Destination directory does not exist", Directory.Exists(destDir), ref globRes);

            // proxy
            var proxy = opts.Proxy;
            _cmdHelper.RegCheck("Proxy class", "Class name is empty", !string.IsNullOrWhiteSpace(proxy?.Class), ref globRes);
            _cmdHelper.RegCheck("Proxy method", "Method name is empty", !string.IsNullOrWhiteSpace(proxy?.Method), ref globRes);

            //profiler (transmiter)
            var profiler = opts.Profiler;
            check = "Profiler directory";
            var profDir = profiler?.Directory;
            if (string.IsNullOrWhiteSpace(profDir))
                _cmdHelper.RegCheck(check, "Directory path is empty", false, ref globRes);
            else
                _cmdHelper.RegCheck(check, $"Directory does not exist: [{profDir}]", Directory.Exists(profDir), ref globRes);

            var profAsmName = profiler?.AssemblyName;
            _cmdHelper.RegCheck("Profiler assembly name", "Assembly name is empty", !string.IsNullOrWhiteSpace(profAsmName), ref globRes);

            var profAsmPath = Path.Combine(profDir, profAsmName);
            _cmdHelper.RegCheck("Profiler assembly file", "Assembly file is not found", File.Exists(profAsmPath), ref globRes);

            _cmdHelper.RegCheck("Profiler namespace", "Namespace is empty", !string.IsNullOrWhiteSpace(profiler?.Namespace), ref globRes);
            _cmdHelper.RegCheck("Profiler class", "Class name is empty", !string.IsNullOrWhiteSpace(profiler?.Class), ref globRes);
            _cmdHelper.RegCheck("Profiler method", "Method name is empty", !string.IsNullOrWhiteSpace(profiler?.Method), ref globRes);

            //TODO: real check by Reflection

            // plugins
            check = "Contexter plugins";
            _cmdHelper.RegCheck(check, "Plugin configuration error", CheckPlugins(check, opts.Plugins), ref globRes);

            // versions
            if (opts.Versions != null)
            {
                check = "Version section";
                _cmdHelper.RegCheck(check, "Version configuration error",
                    _cmdHelper.CheckVersions(check, sourceDir ?? "", opts.Versions), ref globRes);
            }
            //
            _cmdHelper.RegResult(globRes);
            return Task.FromResult(true);
        }

        private bool CheckPlugins(string check, Dictionary<string, PluginLoaderOptions>? plugins)
        {
            var globRes = true;

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
                    _cmdHelper.RegCheck(check, $"Plugin {name}: the directory does not exist: [{dir}]", false, ref globRes);
                    res = false;
                }
                //
                var cfg = plug.Config;
                if (!cfg.EndsWith(".yml"))
                    cfg += ".yml";
                var cfgPath = FileUtils.GetFullPath(Path.Combine(injDir, cfg), dir);
                if (!File.Exists(cfgPath))
                {
                    _cmdHelper.RegCheck(check, $"The specified config does not exist for plugin {name}: [{cfgPath}]", false, ref globRes);
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
