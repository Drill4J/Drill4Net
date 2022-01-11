using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI,
                     //ConfiguratorConstants.CONTEXT_CFG,
                     ConfiguratorConstants.COMMAND_CHECK)]
    public class CiCheckCommand : AbstractConfiguratorCommand
    {
        public CiCheckCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
        {
            if (_desc == null)
                return Task.FromResult(false);

            // open cfg
            var dir = _rep.GetCiDirectory();
            var res = _cmdHelper.GetSourceConfigPath<CiOptions>(CoreConstants.SUBSYSTEM_CI,
                dir, _desc, out var cfgPath, out _, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }
            RaiseMessage($"\nChecking: [{cfgPath}]", CliMessageType.Info);
            //
            var opts = _rep.ReadCiOptions(cfgPath, false);
            string? check;

            //target dirs
            var injection = opts.Injection;
            if (injection == null)
            {
                _cmdHelper.WriteCheck("Injection options", "No injection options", false);
                return Task.FromResult(false);
            }
            else
            {
                var cfgsDir = injection.ConfigDir;
                check = $"Directory with {CoreConstants.SUBSYSTEM_INJECTOR}'s configs";
                if (string.IsNullOrWhiteSpace(cfgsDir))
                {
                    _cmdHelper.WriteCheck(check, $"{check}'s path is empty", false);
                }
                else
                {
                    cfgsDir = FileUtils.GetFullPath(cfgsDir);  //relative Configurator
                    _cmdHelper.WriteCheck(check, $"Path does not exist: [{cfgsDir}]", File.Exists(cfgsDir));
                }
            }

            //Test Runner
            var testRunPath = opts.TestRunnerConfigPath;
            check = $"{CoreConstants.SUBSYSTEM_TEST_RUNNER}'s config";
            if (string.IsNullOrWhiteSpace(testRunPath))
            {
                _cmdHelper.WriteCheck(check, "Path is empty", false);
            }
            else
            {
                testRunPath = FileUtils.GetFullPath(testRunPath); //relative Configurator
                _cmdHelper.WriteCheck(check, $"Path does not exist: [{testRunPath}]", File.Exists(testRunPath));
            }
            //
            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return $"Checks the specified {CoreConstants.SUBSYSTEM_CI}'s configuration before start full workflow.";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}
