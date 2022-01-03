using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Repository;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_LIST)]
    public class TargetListCommand : AbstractConfiguratorCommand
    {
        public TargetListCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /******************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.Options.InjectorDirectory ?? "";
            var configs = GetConfigs(dir)
                .OrderBy(a => a).ToArray();
            var actualCfg = new BaseOptionsHelper(_rep.Subsystem)
                .GetActualConfigPath(dir);
            for (int i = 0; i < configs.Length; i++)
            {
                string? file = configs[i];
                var isActual = file.Equals(actualCfg, StringComparison.InvariantCultureIgnoreCase);
                var a1 = isActual ? "[" : "";
                var a2 = isActual ? "]" : "";
                var name = Path.GetFileNameWithoutExtension(file);
                RaiseMessage($"{a1}{i+1}{a2}. {name}", CliMessageType.Info);
            }
            return Task.FromResult(true);
        }

        internal List<string> GetConfigs(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir))
                throw new Exception("The directory of Injector is empty in config");
            if (!Directory.Exists(dir))
                throw new Exception("The directory of Injector does not exist");
            //
            var allCfgs = Directory.GetFiles(dir, "*.yml");
            var res = new List<string>();
            foreach (var file in allCfgs)
            {
                try
                {
                    var cfg = _rep.ReadInjectorOptions(file);
                    if(cfg?.Type == CoreConstants.SUBSYSTEM_INJECTOR)
                        res.Add(FileUtils.GetFullPath(file, dir));
                }
                catch { }
            }
            return res;
        }
    }
}
