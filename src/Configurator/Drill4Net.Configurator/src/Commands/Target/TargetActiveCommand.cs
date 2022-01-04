using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_ACTIVATE)]
    public class TargetActiveCommand : AbstractConfiguratorCommand
    {
        public TargetActiveCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetInjectorDirectory();

            // source path
            var res = _commandHelper.GetSourceConfig(dir, this, out var sourcePath, out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }

            var path = _rep.CalcRedirectConfigPath(dir);
            var data = _rep.ReadRedirectData(path);
            data.Path = Path.GetFileName(sourcePath); //better set just file name but its path
            _rep.WriteRedirectData(data, path);
            RaiseMessage($"The Injector's config [{sourcePath}] is active now");

            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return "Activate the specified Injector's config";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
