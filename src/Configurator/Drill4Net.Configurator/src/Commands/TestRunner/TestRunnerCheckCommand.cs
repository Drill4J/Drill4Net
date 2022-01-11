using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Agent.TestRunner.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER,
                     //ConfiguratorConstants.CONTEXT_CFG,
                     ConfiguratorConstants.COMMAND_CHECK)]
    public class TestRunnerCheckCommand : AbstractConfiguratorCommand
    {
        public TestRunnerCheckCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public override Task<bool> Process()
        {
            if (_desc == null)
                return Task.FromResult(false);

            // open cfg
            var dir = _rep.GetTestRunnerDirectory();
            var res = _cmdHelper.GetSourceConfigPath<TestRunnerOptions>(CoreConstants.SUBSYSTEM_TEST_RUNNER,
                dir, _desc, out var cfgPath, out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }
            RaiseMessage($"\nChecking: [{cfgPath}]", CliMessageType.Info);
            //
            var opts = _rep.ReadTestRunnerOptions(cfgPath);
            
            //target dirs
            var runDirOpts = opts.Directories;
            if (runDirOpts == null)
            {
                _cmdHelper.WriteCheck("Options", "No options", false);
                return Task.FromResult(false);
            }
            else
            {
                foreach (var runDirOpt in runDirOpts)
                {
                    var runDirA = runDirOpt.Directory;
                    if (string.IsNullOrWhiteSpace(runDirA))
                    {
                        _cmdHelper.WriteCheck("Target directory", "Target directory path is empty", false);
                        continue;
                    }
                    var runDir = FileUtils.GetFullPath(runDirA, _rep.GetTestRunnerDirectory());
                    _cmdHelper.WriteCheck($"Target directory: [{runDir}]", $"Directory does not exist: [{runDir}]", Directory.Exists(runDir));

                    foreach (var asmOpts in runDirOpt.Assemblies)
                    {
                        var asmName = asmOpts.DefaultAssemblyName;
                        if (string.IsNullOrWhiteSpace(asmName)) //it is normal
                            continue;
                        var asmPath = Path.Combine(runDir, asmName);
                        _cmdHelper.WriteCheck($"Test assembly: [{asmPath}]", $"Test assembly does not exist: [{asmPath}]", File.Exists(asmPath));
                    }
                }
            }
            //
            return Task.FromResult(true);
        }

        public override string GetShortDescription()
        {
            return $"Checks the specified {CoreConstants.SUBSYSTEM_TEST_RUNNER}'s configuration before start its tests.";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}
