using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI,
                     //ConfiguratorConstants.CONTEXT_CFG,
                     ConfiguratorConstants.COMMAND_ACTIVE)]
    public class CiActiveCommand : AbstractConfiguratorCommand
    {
        public CiActiveCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetCiDirectory();

            //source path
            var res = _cmdHelper.GetSourceConfigPath<CiOptions>(CoreConstants.SUBSYSTEM_CONFIGURATOR, dir, this,
                out var sourcePath, out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }

            //activate
            var path = _rep.CalcRedirectConfigPath(dir);
            _cmdHelper.SaveRedirectFile(CoreConstants.SUBSYSTEM_CONFIGURATOR,
                Path.GetFileNameWithoutExtension(sourcePath), //better set just file name but its path 
                path);

            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return "Activate the specified CI config";
        }

        public override string GetHelp()
        {
            return "Help article not implemented yet";
        }
    }
}
