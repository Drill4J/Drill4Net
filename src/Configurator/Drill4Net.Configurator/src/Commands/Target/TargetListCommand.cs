using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute("trg", "cfg", "list")]
    public class TargetListCommand : AbstractConfiguratorCommand
    {
        public TargetListCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /******************************************************************/

        public override Task<bool> Process()
        {
            var configs = GetConfigs(_rep.Options.InjectorDirectory ?? "")
                .OrderBy(a => a).ToArray();
            for (int i = 0; i < configs.Length; i++)
            {
                string? file = configs[i];
                var name = Path.GetFileNameWithoutExtension(file);
                RaiseMessage($"{i+1}. {name}", CliMessageType.Info);
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
                    if(cfg != null && cfg.Type == CoreConstants.SUBSYSTEM_INJECTOR)
                        res.Add(file);
                }
                catch { }
            }
            return res;
        }
    }
}
