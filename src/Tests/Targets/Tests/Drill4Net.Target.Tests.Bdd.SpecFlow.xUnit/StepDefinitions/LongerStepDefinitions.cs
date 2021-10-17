using System;
using Xunit;
using FluentAssertions;
using TechTalk.SpecFlow;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xunit.Abstractions;
using Drill4Net.Target.Testers.Common;
using Drill4Net.Target.Tests.Bdd.SpecFlow.xUnit.Logging;

//https://xunit.net/docs/running-tests-in-parallel
[assembly: CollectionBehavior(DisableTestParallelization = false, MaxParallelThreads = 8)] //Default: false

namespace Drill4Net.Target.Tests.Bdd.SpecFlow.xUnit.StepDefinitions
{
    [Binding]
    public class LongerStepDefinitions
    {
        private readonly Longer _longer;

        /// <summary>
        /// Additional logging reporter (it is not context isself). Need for DI registering
        /// </summary>
        private readonly TestRunnerReporter _rep = new TestRunnerReporter();

        /************************************************************************************/

        public LongerStepDefinitions()
        {
            _longer = new Longer();
        }

        /************************************************************************************/

        //https://github.com/xunit/xunit/issues/621 - they say, no test context in xUnit. It is sad.
        // but in the discussion above and in the source (as silly class) it exists (not in NuGet package - coomin on 23 Jule, 2021):
        //https://github.com/xunit/xunit/blob/32a168c759e38d25931ee91925fa75b6900209e1/src/xunit.v3.core/Sdk/Frameworks/TestContextAccessor.cs

        //DON'T REMOVE THIS EVEN IF IT IS COMMENTED

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

        [BeforeScenario(Order = 0)]
        public static void DebugScenarioStarting(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            var feature = $"{featureContext.FeatureInfo.FolderPath}/{featureContext.FeatureInfo.Title}";
            var scenario = scenarioContext.ScenarioInfo.Title;
            var key = $"{feature}^{scenario}";
        }

        [AfterScenario(Order = 0)]
        public static void DebugScenarioFinished(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            var feature = $"{featureContext.FeatureInfo.FolderPath}/{featureContext.FeatureInfo.Title}";
            var scenario = scenarioContext.ScenarioInfo.Title;
            var testStatus = scenarioContext.ScenarioExecutionStatus;
            var testError = scenarioContext.TestError;
        }

        /************************************************************************************/

        [When("do long work for (.*)")]
        public void WaitTimeout(int timeout)
        {
            _longer.DoLongWork(timeout);
        }

        [When("do default long work")]
        public void DoDefaultAction()
        {
            _longer.DoLongWork();
        }
    }
}
