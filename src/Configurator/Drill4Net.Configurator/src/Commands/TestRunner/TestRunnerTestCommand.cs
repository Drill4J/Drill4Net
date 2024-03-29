﻿using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Agent.TestRunner.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER, ConfiguratorConstants.COMMAND_TEST)]
    public class TestRunnerTestCommand : AbstractConfiguratorCommand
    {
        public TestRunnerTestCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /****************************************************************/

        public async override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return FalseEmptyResult;
            //
            var dir = _rep.GetTestRunnerDirectory();
            var res2 = _cmdHelper.GetExistingSourceConfigPath<TestRunnerOptions>(CoreConstants.SUBSYSTEM_TEST_RUNNER,
                dir, _desc, out var runCfgPath, out var _);
            if (!res2)
                return FalseEmptyResult;

            var (res, err) = await _cmdHelper.TestRunnerProcess(this, runCfgPath)
                .ConfigureAwait(false);
            if (!res)
            {
                RaiseError(err);
                return FalseEmptyResult;
            }
            return TrueEmptyResult;
        }

        public override string GetShortDescription()
        {
            return "Run automated tests in the instrumented target.";
        }

        public override string GetHelp()
        {
            return @$"The separated from the full CI pipeline process of the start the tests in the specified instrumented target by config.

{HelpHelper.GetActiveLastSwitchesDesc(CoreConstants.SUBSYSTEM_TEST_RUNNER, RawContexts)}

Also you can use config path directly:
    Example: {RawContexts} -- ""d:\Drill4Net\ci\targetA\run.yml""
    Example: {RawContexts} --{CoreConstants.ARGUMENT_CONFIG_PATH}=""d:\Drill4Net\ci\targetA\run.yml""

If you need to restart all tests on the current build, apply the switch ""{CoreConstants.SWITCH_FORCE_RUNNIG_TYPE_ALL}"" (with a capital letter).";
        }
    }
}
