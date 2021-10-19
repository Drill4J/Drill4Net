using System;
using FluentAssertions;
using TechTalk.SpecFlow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Drill4Net.Target.Testers.Common;

//https://docs.specflow.org/projects/specflow/en/latest/Execution/Parallel-Execution.html
//https://stackoverflow.com/questions/3917060/how-to-run-unit-tests-mstest-in-parallel
//https://docs.microsoft.com/ru-ru/visualstudio/test/configure-unit-tests-by-using-a-dot-runsettings-file?view=vs-2019
[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.ClassLevel)]
//[DoNotParallelize] - to some test

namespace Drill4Net.Target.Tests.Bdd.SpecFlow.MsTest.StepDefinitions
{
    [Binding]
    public class LongerStepDefinitions
    {
        private readonly Longer _longer;

        /************************************************************************************/

        public LongerStepDefinitions()
        {
            _longer = new Longer();
        }

        /************************************************************************************/

        //DON'T REMOVE THIS EVEN IF IT IS COMMENTED

        [BeforeScenario(Order = 0)]
        public static void DebugScenarioStarting(FeatureContext featureContext, ScenarioContext scenarioContext, TestContext testCtx)
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
