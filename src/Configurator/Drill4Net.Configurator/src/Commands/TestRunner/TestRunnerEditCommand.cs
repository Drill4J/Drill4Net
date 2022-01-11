﻿using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Agent.TestRunner.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER,
                         //ConfiguratorConstants.CONTEXT_CFG,
                         ConfiguratorConstants.COMMAND_EDIT)]
    public class TestRunnerEditCommand : AbstractTestRunnerEditor
    {
        public TestRunnerEditCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /***********************************************************************/

        public override Task<bool> Process()
        {
            if (_desc == null)
                return Task.FromResult(false);

            var dir = _rep.GetTestRunnerDirectory();

            // source path
            var res = _cmdHelper.GetSourceConfigPath<TestRunnerOptions>(CoreConstants.SUBSYSTEM_TEST_RUNNER, dir, _desc,
                out var sourcePath, out var _, out var error);
            if (!res)
            {
                RaiseError(error);
                return Task.FromResult(false);
            }

            res = Edit(sourcePath, false);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return $"Edit the specified {CoreConstants.SUBSYSTEM_TEST_RUNNER}'s config in interactive mode.";
        }

        public override string GetHelp()
        {
            return "";
        }
    }
}
