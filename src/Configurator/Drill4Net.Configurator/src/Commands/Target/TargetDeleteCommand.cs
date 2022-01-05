using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_TARGET,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_DELETE)]
    public class TargetDeleteCommand : AbstractConfiguratorCommand
    {
        public TargetDeleteCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetInjectorDirectory();

            // source path
            var res = _cmdHelper.GetSourceConfig<InjectorOptions>(CoreConstants.SUBSYSTEM_INJECTOR, dir, this,
                out var sourcePath, out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }

            // ask
            var forceDelete = IsSwitchSet('f');
            if (!forceDelete)
            {
                //to delete the actual config in redirecting file is bad idea
                var actualCfg = _rep.GetActualConfigPath(dir);
                string answer;
                if (actualCfg.Equals(sourcePath, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!_cli.AskQuestion($"The config [{sourcePath}] is active in the redirecting file.\nDo you want to delete it? Answer", out answer, "n"))
                        return Task.FromResult(false);
                    if (!_cli.IsYes(answer))
                        return Task.FromResult(false);
                }
                //
                if (!_cli.AskQuestion($"Delete the file [{sourcePath}]?", out answer, "y"))
                    return Task.FromResult(false);
                if (!_cli.IsYes(answer))
                    return Task.FromResult(false);
            }

            //output
            File.Delete(sourcePath);
            RaiseMessage($"Config is deleted: [{sourcePath}]");

            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return $"Delete the specified {CoreConstants.SUBSYSTEM_INJECTOR}'s config";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
