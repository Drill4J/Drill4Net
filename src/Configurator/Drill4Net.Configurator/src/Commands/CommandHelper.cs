using System;
using System.IO;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    public class CommandHelper
    {
        private readonly ConfiguratorRepository _rep;

        /*****************************************************************/

        public CommandHelper(ConfiguratorRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
        }

        /*****************************************************************/

        internal bool GetSourceConfig(string dir, AbstractCliCommand cmd, out string path, out bool fromSwitch, out string error)
        {
            var sourceName = string.Empty;
            path = string.Empty;
            error = string.Empty;

            //switches
            var copyActive = cmd.IsSwitchSet('a'); //copy active
            if (copyActive)
            {
                var actualCfg = _rep.GetActualConfigPath(dir);
                sourceName = Path.GetFileName(actualCfg);
            }
            if (sourceName == string.Empty)
            {
                var copyLast = cmd.IsSwitchSet('l');
                if (copyLast)
                {
                    var configs = _rep.GetInjectorConfigs(dir);
                    var lastEditedFile = string.Empty;
                    var dt = DateTime.MinValue;
                    foreach (var config in configs)
                    {
                        var fdt = File.GetLastWriteTime(config);
                        if (fdt < dt)
                            continue;
                        dt = fdt;
                        lastEditedFile = config;
                    }
                    if (lastEditedFile != string.Empty)
                        sourceName = Path.GetFileName(lastEditedFile);
                }
            }

            fromSwitch = !string.IsNullOrWhiteSpace(sourceName);

            if (string.IsNullOrWhiteSpace(sourceName))
                sourceName = cmd.GetPositional(0);

            //source path
            return GetConfigPath(dir, "source", sourceName, true, out path, out error);
        }

        internal bool GetConfigPath(string dir, string typeConfig, string name, bool mustExist, out string path, out string error)
        {
            path = string.Empty;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(name))
            {
                error = $"The {typeConfig} config is not specified, see help.";
                return false;
            }
            if (!string.IsNullOrWhiteSpace(Path.GetDirectoryName(name)))
            {
                error = $"The {typeConfig} config should be just a file name without a directory, see help.";
                return false;
            }
            if (!name.EndsWith(".yml"))
                name += ".yml";

            path = Path.Combine(dir, name);
            if (mustExist && !File.Exists(path))
            {
                error = $"The {typeConfig} config not found: [{path}]";
                return false;
            }

            return true;
        }
    }
}
