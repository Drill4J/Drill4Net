﻿using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Agent.TestRunner.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER, ConfiguratorConstants.COMMAND_OPEN)]
    public class TestRunnerOpenCommand : AbstractConfiguratorCommand
    {
        public TestRunnerOpenCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*****************************************************************/

        public override Task<bool> Process()
        {
            var dir = _rep.GetTestRunnerDirectory();
            var res = _cmdHelper.OpenConfig<TestRunnerOptions>(CoreConstants.SUBSYSTEM_TEST_RUNNER, dir, this);
            return Task.FromResult(res);
        }

        public override string GetShortDescription()
        {
            return $"Open in external editor the config for {CoreConstants.SUBSYSTEM_TEST_RUNNER}";
        }

        public override string GetHelp()
        {
            return $"Help article not implemented yet";
        }
    }
}
