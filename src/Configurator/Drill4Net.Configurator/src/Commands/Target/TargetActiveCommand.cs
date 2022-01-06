using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_ACTIVE)]
    public class TargetActiveCommand : AbstractConfiguratorCommand
    {
        public TargetActiveCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /******************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetInjectorDirectory();

            //source path
            var res = _cmdHelper.GetSourceConfig<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR, dir, this,
                out var sourcePath, out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }

            //activate
            var path = _rep.CalcRedirectConfigPath(dir);
            _cmdHelper.SaveRedirectFile(CoreConstants.SUBSYSTEM_INJECTOR,
                Path.GetFileNameWithoutExtension(sourcePath), //better set just file name but its path 
                path);

            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return $"Activate the specified {CoreConstants.SUBSYSTEM_INJECTOR}'s config";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
