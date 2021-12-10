using System.Reflection;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Drill4Net.Target.Frameworks.Common;

// automatic version tagger including Git info - https://github.com/devlooped/GitInfo
// semVer creates an automatic version number based on the combination of a SemVer-named tag/branches
// the most common format is v0.0 (or just 0.0 is enough)
// to change semVer it is nesseccary to create appropriate tag and push it to remote repository
// patches'(commits) count starts with 0 again after new tag pushing
// For file version format exactly is digit
[assembly: AssemblyFileVersion($"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}")]
[assembly: AssemblyInformationalVersion($"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}-{ThisAssembly.Git.Branch}+{ThisAssembly.Git.Commit}")]

//Project mustn't have in its name "NUnit" part, only "nUnit" - with small letter "n"

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
