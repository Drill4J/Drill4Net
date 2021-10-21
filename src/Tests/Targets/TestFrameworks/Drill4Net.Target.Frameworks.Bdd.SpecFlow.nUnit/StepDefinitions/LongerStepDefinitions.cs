using TechTalk.SpecFlow;
using Drill4Net.Target.Frameworks.Common;
using NUnit.Framework;

//https://docs.specflow.org/projects/specflow/en/latest/Execution/Parallel-Execution.html
//https://docs.nunit.org/articles/nunit/writing-tests/attributes/parallelizable.html
//https://docs.nunit.org/articles/nunit/writing-tests/attributes/levelofparallelism.html

[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(4)]


namespace Drill4Net.Target.Frameworks.Bdd.SpecFlow.nUnit.StepDefinitions
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
