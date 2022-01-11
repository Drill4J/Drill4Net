using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Configuration;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_CHECK)]
    public class TargetCheckCommand : AbstractConfiguratorCommand
    {
        public TargetCheckCommand(ConfiguratorRepository rep) : base(rep)
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
            RaiseMessage("\nChecks:");

            var destDir = opts.Destination.Directory ?? "";
            string check;

            //target
            var target = opts.Target;
            write("Target name", "Name is empty", !string.IsNullOrWhiteSpace(target.Name));
            if (!string.IsNullOrWhiteSpace(target.Version))
            {
                write("Target version", "", true);
            }
            else
            {
                var asmName = target.VersionAssemblyName;
                if (!string.IsNullOrWhiteSpace(asmName))
                {
                    check = "Target version assembly";
                    var asmPath = Path.Combine(destDir, asmName);
                    write(check, $"The assembly for determining the target version was not found: [{asmPath}]", File.Exists(asmPath));
                }
                else
                {
                    write("Target versioning", "", false);
                }
            }

            //source
            var source = opts.Source;

            check = "Source directory";
            var sourceDir = source?.Directory;
            if (string.IsNullOrWhiteSpace(sourceDir))
                write(check, "Source directory path is empty", false);
            else
                write(check, $"Source directory does not exist: [{sourceDir}]", Directory.Exists(sourceDir));

            //filter
            res = IsFilterPartOk(source?.Filter?.Includes) || IsFilterPartOk(source?.Filter?.Excludes);
            write("Filter for injected entities", "No filter entry", res);

            //destination
            check = "Destination directory";
            if (string.IsNullOrWhiteSpace(destDir))
                write(check, "Destination directory path is empty", false);
            else
                write(check, "Destination directory does not exist", Directory.Exists(destDir));

            // proxy
            var proxy = opts.Proxy;
            write("Proxy class", "Class name is empty", !string.IsNullOrWhiteSpace(proxy?.Class));
            write("Proxy method", "Method name is empty", !string.IsNullOrWhiteSpace(proxy?.Method));

            //profiler (transmiter)
            var profiler = opts.Profiler;
            check = "Profiler directory";
            var profDir = profiler?.Directory;
            if (string.IsNullOrWhiteSpace(profDir))
                write(check, "Directory path is empty", false);
            else
                write(check, $"Directory does not exist: [{profDir}]", Directory.Exists(profDir));

            var profAsmName = profiler?.AssemblyName;
            write("Profiler assembly name", "Assembly name is empty", !string.IsNullOrWhiteSpace(profAsmName));

            var profAsmPath = Path.Combine(profDir, profAsmName);
            write("Profiler assembly file", "Assembly file is not found", File.Exists(profAsmPath));

            write("Profiler namespace", "Namespace is empty", !string.IsNullOrWhiteSpace(profiler?.Namespace));
            write("Profiler class", "Class name is empty", !string.IsNullOrWhiteSpace(profiler?.Class));
            write("Profiler method", "Method name is empty", !string.IsNullOrWhiteSpace(profiler?.Method));

            //TODO: real check by Reflection

            // plugins
            check = "Contexter plugins";
            write(check, "Plugin configuration error", CheckPlugins(check, opts.Plugins));

            // versions
            if (opts.Versions != null)
            {
                check = "Version section";
                write(check, "Version configuration error", CheckVersions(check, sourceDir ?? "", opts.Versions));
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
                    write(check, $"Plugin {name}: the directory does not exist: [{dir}]", false);
                    res = false;
                }
                //
                var cfg = plug.Config;
                if (!cfg.EndsWith(".yml"))
                    cfg += ".yml";
                var cfgPath = FileUtils.GetFullPath(Path.Combine(injDir, cfg), dir);
                if (!File.Exists(cfgPath))
                {
                    write(check, $"The specified config does not exist for plugin {name}: [{cfgPath}]", false);
                    res = false;
                }
            }
            return res;
        }

        private bool CheckVersions(string check, string sourceDir, VersionData? versData)
        {
            // section can be empty
            if (versData == null)
                return true;
            if (string.IsNullOrWhiteSpace(sourceDir))
            {
                write(check, "Can't check the version section due to source directory path is empty", false);
                return false;
            }
            //
            var res = true;
            //var dir = versData.Directory; //used only for tests' system
            foreach (var moniker in versData.Targets.Keys)
            {
                var monData = versData.Targets[moniker];
                
                // base dir
                var rootDir = Path.Combine(monData.BaseFolder, sourceDir);
                var res2 = Directory.Exists(rootDir);
                if (!res2)
                {
                    write(check, $"Directory for {moniker} does not exist: [{rootDir}]", false);
                    res = false;
                }

                // folders (used fot Test Engine, not for common targets)
                if (monData.Folders != null)
                {
                    foreach (var fldData in monData.Folders)
                    {
                        var asmFld = fldData.Folder; //can be empty
                        if (string.IsNullOrWhiteSpace(asmFld))
                            asmFld = rootDir;
                        //
                        var asmDir = FileUtils.GetFullPath(asmFld, rootDir);
                        res2 = Directory.Exists(asmDir);
                        if (!res2)
                        {
                            write(check, $"Directory for {moniker} and folder {asmFld} does not exist: [{asmDir}]", false);
                            res = false;
                        }

                        //assemblies
                        if (fldData.Assemblies != null)
                        {
                            foreach (var asmName in fldData.Assemblies.Keys)
                            {
                                var asmPath = Path.Combine(asmDir, asmName);
                                res2 = File.Exists(asmPath);
                                if (!res2)
                                {
                                    write(check, $"Assembly for {moniker} and folder {asmFld} not found: [{asmPath}]", false);
                                    res = false;
                                }
                                //
                                //var types = fldData.Assemblies[asmName]; //We are not checking this yet
                            }
                        }
                    }
                }
            }
            return res;
        }

        void write(string check, string error, bool res)
        {
            if (res)
            {
                RaiseMessage($"{check}: OK", CliMessageType.Info);
            }
            else
            {
                RaiseError($"{check}: NOT");
                RaiseError(error);
            }
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
