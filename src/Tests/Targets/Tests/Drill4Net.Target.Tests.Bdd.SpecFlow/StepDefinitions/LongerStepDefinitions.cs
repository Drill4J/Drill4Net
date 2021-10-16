using Xunit;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Drill4Net.Target.Testers.Common;

//https://xunit.net/docs/running-tests-in-parallel
[assembly: CollectionBehavior(DisableTestParallelization = false, MaxParallelThreads = 8)] //Default: false

namespace Drill4Net.Target.Tests.Bdd.SpecFlow.StepDefinitions
{
    [Binding]
    public class LongerStepDefinitions
    {
        private readonly Longer _longer;

        /*******************************************************/

        public LongerStepDefinitions()
        {
            _longer = new Longer();
        }

        /*******************************************************/

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
