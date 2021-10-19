using Xunit;
using Xunit.Abstractions;

namespace Drill4Net.Target.Tests.Bdd.SpecFlow.xUnit.Logging
{
    //Need xunit.runner.utility NuGet package
    //https://stackoverflow.com/questions/54867744/c-sharp-xunit-test-listeners/54973118#54973118

    /// <summary>
    /// Reporter for the xUnit actions for additional LOGGING (it is not context itself)
    /// </summary>
    internal class TestRunnerReporter : IRunnerReporter
    {
        public string Description => "My custom runner reporter";

        // Hard-coding `true` means this reporter will always be enabled.
        //
        // You can also implement logic to conditional enable/disable the reporter.
        // Most reporters based this decision on an environment variable.
        // Eg: https://github.com/xunit/xunit/blob/cbf28f6d911747fc2bcd64b6f57663aecac91a4c/src/xunit.runner.reporters/TeamCityReporter.cs#L11
        public bool IsEnvironmentallyEnabled => true;

        public string RunnerSwitch => "mycustomrunnerreporter";

        public IMessageSink CreateMessageHandler(IRunnerLogger logger)
        {
            return new TestMessageSink();
        }
    }
}
