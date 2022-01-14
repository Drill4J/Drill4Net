using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_CHECK)]
    public class CiCheckCommand : AbstractConfiguratorCommand
    {
        public CiCheckCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /**************************************************************************/

        public override async Task<(bool done, Dictionary<string, object> results)> Process()
        {
            var cmdRes = true;
            RaiseMessage($"\n{CoreConstants.SUBSYSTEM_CI} configuration check.", CliMessageType.Info);

            if (_desc == null)
                return FalseEmptyResult;

            var force = IsSwitchSet(CoreConstants.SWITCH_FORCE); //to check all the specified configurations

            // open cfg
            var dir = _rep.GetCiDirectory();
            var res = _cmdHelper.GetSourceConfigPath<CiOptions>(CoreConstants.SUBSYSTEM_CI,
                dir, _desc, out var cfgPath, out _, out var error);
            if (!res)
            {
                RaiseError(error);
                return FalseEmptyResult;
            }
            RaiseMessage($"Checking: [{cfgPath}]", CliMessageType.Info);
            //
            var opts = _rep.ReadCiOptions(cfgPath, false);
            string? check;

            //target dirs
            var injection = opts.Injection;
            if (injection == null)
            {
                _cmdHelper.RegCheck("Injection options", "No injection options", false, ref cmdRes);
                return FalseEmptyResult;
            }
            else
            {
                var cfgsDir = injection.ConfigDir;
                check = $"Directory with {CoreConstants.SUBSYSTEM_INJECTOR} configs";
                if (string.IsNullOrWhiteSpace(cfgsDir))
                {
                    _cmdHelper.RegCheck(check, $"{check}'s path is empty", false, ref cmdRes);
                }
                else
                {
                    cfgsDir = FileUtils.GetFullPath(cfgsDir);  //relative to Configurator
                    var cfgExists = Directory.Exists(cfgsDir);
                    _cmdHelper.RegCheck(check, $"Path does not exist: [{cfgsDir}]", cfgExists, ref cmdRes);
                    if(force && cfgExists)
                    {
                        var checkTrgCmd = _cliRep.GetCommand(typeof(TargetCheckCommand));
                        if (checkTrgCmd != null)
                        {
                            var configs = Directory.GetFiles(cfgsDir, "*.yml");
                            if (configs.Length > 0)
                            {
                                foreach (var config in configs)
                                {
                                    _cli.DrawLine();
                                    var desc = new CliDescriptor(@$"{CoreConstants.ARGUMENT_CONFIG_PATH}=""{config}""", false);
                                    var trgRes = await ProcessFor(checkTrgCmd, desc);
                                    _cmdHelper.SetCommandCheckResult(trgRes, ref cmdRes);
                                }
                                _cli.DrawLine();
                            }
                        }
                    }
                }
            }

            //Test Runner
            var testRunPath = opts.TestRunnerConfigPath;
            check = $"{CoreConstants.SUBSYSTEM_TEST_RUNNER} config";
            if (string.IsNullOrWhiteSpace(testRunPath))
            {
                _cmdHelper.RegCheck(check, "Path is empty", false, ref cmdRes);
            }
            else
            {
                testRunPath = FileUtils.GetFullPath(testRunPath); //relative to Configurator
                var cfgExists = File.Exists(testRunPath);
                _cmdHelper.RegCheck(check, $"Path does not exist: [{testRunPath}]", cfgExists, ref cmdRes);
                if (force && cfgExists)
                {
                    var checkTrgCmd = _cliRep.GetCommand(typeof(TestRunnerCheckCommand));
                    if (checkTrgCmd != null)
                    {
                        _cli.DrawLine();
                        var desc = new CliDescriptor(@$"{CoreConstants.ARGUMENT_CONFIG_PATH}=""{testRunPath}""", false);
                        var runnerRes = await ProcessFor(checkTrgCmd, desc);
                        _cmdHelper.SetCommandCheckResult(runnerRes, ref cmdRes);
                        _cli.DrawLine();
                    }
                }
            }
            //
            _cmdHelper.RegResult(cmdRes, true);
            return cmdRes ? OkCheck : NotCheck;
        }

        public override string GetShortDescription()
        {
            return $"Checks the specified {CoreConstants.SUBSYSTEM_CI} configuration before starting the full workflow.";
        }

        public override string GetHelp()
        {
            return $@"This command allows you to check the {CoreConstants.SUBSYSTEM_CI} configuration for soame ""run"" before the actual processing. The correctness and integrity of parameter values, file paths to targets, specified {CoreConstants.SUBSYSTEM_TEST_RUNNER} executable, and the like are automatically checked. A report is displayed at the end.

{HelpHelper.GetArgumentsForSourceConfig(CoreConstants.SUBSYSTEM_CI, RawContexts, "ci")}

You can use the ""f"" switch to check not only the {CoreConstants.SUBSYSTEM_CI} configuration, but also all related configurations (for the injections and the {CoreConstants.SUBSYSTEM_TEST_RUNNER}).
    Example: ci check -lf";
        }
    }
}
