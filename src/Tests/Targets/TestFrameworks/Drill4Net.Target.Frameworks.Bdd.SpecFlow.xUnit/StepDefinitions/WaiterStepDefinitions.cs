using System.Reflection;
using TechTalk.SpecFlow;
using Xunit;
using Drill4Net.Target.Frameworks.Common;

        /* *
         * 
         * NEVER!!!! NEVER!!!! NEVER INCLUDE ANY DRILL4NET DEPENDENCIES TO THIS PROJECT (except specific common tests' ones)
         * Because they will be injected and then conflict with depenedncies from Transmitter, Agents and so on in memory
         * 
         * */

//http://gasparnagy.com/2016/02/running-specflow-scenarios-in-parallel-with-xunit-v2/
//https://github.com/Microsoft/vstest-docs/blob/master/docs/configure.md

/***************************************************************************************
* 
* In xUnit 2.4.x the TestContest is not exists
* In xUnit 3.x the TestContext is exists (commit on 23 July, 2021) but this version 
* in alpha state in late 2021
* So, all tests for xUnit 2.4.x must be run in serial, not parallel mode
* Run from console with misc modes: https://xunit.net/docs/running-tests-in-parallel &
* https://xunit.net/docs/getting-started/netfx/cmdline, e.g.
* c:\Users\Ivan_Bezrodnyi\.nuget\packages\xunit.runner.console\2.4.1\tools\net472\xunit.console.exe Drill4Net.Target.Frameworks.Bdd.SpecFlow.xUnit.dll -parallel none
* dotnet test Drill4Net.Target.Frameworks.Bdd.SpecFlow.xUnit.dll -- RunConfiguration.DisableParallelization=true
* 
****************************************************************************************/

//https://docs.specflow.org/projects/specflow/en/latest/Execution/Parallel-Execution.html

// automatic version tagger including Git info - https://github.com/devlooped/GitInfo
// semVer creates an automatic version number based on the combination of a SemVer-named tag/branches
// the most common format is v0.0 (or just 0.0 is enough)
// to change semVer it is nesseccary to create appropriate tag and push it to remote repository
// patches'(commits) count starts with 0 again after new tag pushing
// For file version format exactly is digit
[assembly: AssemblyFileVersion($"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}")]
[assembly: AssemblyInformationalVersion($"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}-{ThisAssembly.Git.Branch}+{ThisAssembly.Git.Commit}")]

//https://xunit.net/docs/running-tests-in-parallel
[assembly: CollectionBehavior(DisableTestParallelization = false, //Default: false
                              MaxParallelThreads = 8)]

namespace Drill4Net.Target.Frameworks.Bdd.SpecFlow.xUnit.StepDefinitions
{
    [Binding]
    public class WaiterStepDefinitions
    {
        private readonly Waiter _waiter;

        ///// <summary>
        ///// Additional logging reporter for investigating mostly (it is not context isself). 
        ///// Need for DI registering
        ///// </summary>
        //private readonly TestRunnerReporter _rep = new();

        /************************************************************************************/

        public WaiterStepDefinitions()
        {
            _waiter = new Waiter();
        }

        /************************************************************************************/

        // https://github.com/xunit/xunit/issues/621 - they say, no test context in xUnit 2.4.x now. It is sad.
        // but in the discussion above and in the source xUnit 3.x (as silly class) it exists (not in NuGet package - commit on 23 Jule, 2021):
        // https://github.com/xunit/xunit/blob/32a168c759e38d25931ee91925fa75b6900209e1/src/xunit.v3.core/Sdk/Frameworks/TestContextAccessor.cs

        #region Debug code

        //DON'T REMOVE THIS EVEN IF IT IS COMMENTED

        //[BeforeTestRun]
        //public static void VanchoTestsTestsStarted()
        //{ }

        //[AfterTestRun]
        //public static void VanchoTestsFinished()
        //{ }

        //[BeforeTestRun(Order = 0)]
        //private static void BeforeTestStarting(ITestRunnerManager testRunnerManager, ITestRunner testRunner)
        //{
        //    //All parameters are resolved from the test thread container automatically.
        //    //Since the global container is the base container of the test thread container, globally registered services can be also injected.

        //    //ITestRunManager from global container
        //    var location = testRunnerManager.TestAssembly.Location;

        //    //ITestRunner from test thread container
        //    var threadId = testRunner.ThreadId;
        //}

        //[BeforeScenario(Order = 0)]
        //public static void DebugScenarioStarting(FeatureContext featureContext, ScenarioContext scenarioContext)
        //{
        //    var feature = $"{featureContext.FeatureInfo.FolderPath}/{featureContext.FeatureInfo.Title}";
        //    var scenario = scenarioContext.ScenarioInfo.Title;
        //    var key = $"{feature}^{scenario}";
        //    scenarioContext["TestCase"] = key;
        //}

        //[AfterScenario(Order = 0)]
        //public static void DebugScenarioFinished(FeatureContext featureContext, ScenarioContext scenarioContext)
        //{
        //    var feature = $"{featureContext.FeatureInfo.FolderPath}/{featureContext.FeatureInfo.Title}";
        //    var scenario = scenarioContext.ScenarioInfo.Title;
        //    var testStatus = scenarioContext.ScenarioExecutionStatus;
        //    var testError = scenarioContext.TestError;

        //    var key = scenarioContext["TestCase"];
        //}
        #endregion

        /************************************************************************************/

        [When("do long work for (.*)")]
        public void WaitTimeout(int timeout)
        {
            _waiter.Wait(timeout);
        }

        [When("do default long work")]
        public void DoDefaultAction()
        {
            _waiter.Wait();
        }
    }
}
