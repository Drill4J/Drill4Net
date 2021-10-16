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
        private int _timeout;

        /*******************************************************/

        public LongerStepDefinitions()
        {
            _longer = new Longer();
        }

        /*******************************************************/

        [Given("the timeout is (.*)")]
        public void GivenTimeout(int timeout)
        {
            _timeout = timeout;
        }

        [When("do long work")]
        public Task<int> WaitTimeout()
        {
            return Task.Run(() => Task.FromResult(_longer.DoLongWork(_timeout)));
        }

        [When("do default long work")]
        public Task<int> DoDefaultAction()
        {
            return Task.Run(() => Task.FromResult(_longer.DoLongWork()));
        }
    }
}
