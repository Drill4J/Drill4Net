using System;

namespace Drill4Net.Configurator
{
    public abstract class TestRunnerEditor : AbstractInteractiveCommand
    {
        protected TestRunnerEditor(ConfiguratorRepository rep) : base(rep)
        {
        }
    }
}
