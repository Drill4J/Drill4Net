using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Agent.TestRunner.Core;
using System.Collections.Generic;
using Drill4Net.Repository;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER,
                        //ConfiguratorConstants.CONTEXT_CFG,
                        ConfiguratorConstants.COMMAND_CHECK)]
    public class TestRunnerCheckCommand : AbstractConfiguratorCommand
    {
        public TestRunnerCheckCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /**************************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            var cmdRes = true;
            RaiseMessage($"\n{CoreConstants.SUBSYSTEM_TEST_RUNNER} configuration check.", CliMessageType.Info);

            //open cfg
            var cfgPath = GetParameter(CoreConstants.ARGUMENT_CONFIG_PATH);
            if (string.IsNullOrWhiteSpace(cfgPath))
            {
                if (_desc == null)
                    return Task.FromResult(FalseEmptyResult);

                var dir = _rep.GetTestRunnerDirectory();
                var res = _cmdHelper.GetSourceConfigPath<TestRunnerOptions>(CoreConstants.SUBSYSTEM_TEST_RUNNER,
                    dir, _desc, out cfgPath, out var _, out var error);
                if (!res)
                {
                    RaiseError(error);
                    return Task.FromResult(FalseEmptyResult);
                }
            }
            else
            {
                if (!File.Exists(cfgPath))
                {
                    RaiseError($"Specified by parameter config is not found: [{cfgPath}]");
                    return Task.FromResult(FalseEmptyResult);
                }
            }
            //
            RaiseMessage($"Checking: [{cfgPath}]", CliMessageType.Info);
            _cli.DrawShortSeparator();

            var opts = _rep.ReadTestRunnerOptions(cfgPath);
            
            //target dirs
            var runDirOpts = opts.Directories;
            if (runDirOpts == null)
            {
                _cmdHelper.RegCheck("Directory options", "No directory options", false, ref cmdRes);
                return Task.FromResult(FalseEmptyResult);
            }
            else
            {
                var treeHelper = new TreeRepositoryHelper(CoreConstants.SUBSYSTEM_CONFIGURATOR);
                foreach (var dirOpts in runDirOpts)
                {
                    var runDir = dirOpts.Directory;
                    if (string.IsNullOrWhiteSpace(runDir))
                    {
                        _cmdHelper.RegCheck("Target directory", "Target directory path is empty", false, ref cmdRes);
                        continue;
                    }

                    var fullDir = FileUtils.GetFullPath(runDir, _rep.GetTestRunnerDirectory());
                    var dirExists = Directory.Exists(fullDir);
                    _cmdHelper.RegCheck($"Target directory: [{fullDir}]", $"Directory does not exist: [{fullDir}]",
                        dirExists, ref cmdRes);

                    if (dirExists)
                    {
                        //tree file
                        var treePath = treeHelper.CalculateTreeFilePath(fullDir);
                        _cmdHelper.RegCheck("Tree metadata file", $"Tree metadata file does not exist, check injection process: [{treePath}]",
                            File.Exists(treePath), ref cmdRes);

                        //assemblies
                        foreach (var asmOpts in dirOpts.Assemblies)
                        {
                            var asmName = asmOpts.DefaultAssemblyName;
                            if (string.IsNullOrWhiteSpace(asmName)) //it is normal
                                continue;
                            var asmPath = Path.Combine(fullDir, asmName);
                            _cmdHelper.RegCheck($"Test assembly: [{asmName}]", $"Test assembly does not exist: [{asmPath}]",
                                File.Exists(asmPath), ref cmdRes);
                        }
                    }
                    _cli.DrawShortSeparator();
                }
            }
            //
            _cmdHelper.RegResult(cmdRes);
            return Task.FromResult(cmdRes ? OkCheck : NotCheck);
        }

        public override string GetShortDescription()
        {
            return $"Checks the specified {CoreConstants.SUBSYSTEM_TEST_RUNNER} configuration before start its tests.";
        }

        public override string GetHelp()
        {
            return "The article has not been written yet";
        }
    }
}
