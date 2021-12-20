using FluentAssertions;
using TechTalk.SpecFlow;
using Drill4Net.Target.Frameworks.Common;
using System.Threading.Tasks;

namespace Drill4Net.Target.Frameworks.Bdd.SpecFlow.xUnit.StepDefinitions
{
    [Binding]
    public sealed class CalculatorStepDefinitions
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext _scenarioCtx;
        private readonly Calculator _calculator;

        /************************************************************************************/

        public CalculatorStepDefinitions(ScenarioContext scenarioCtx)
        {
            _scenarioCtx = scenarioCtx;
            _calculator = new Calculator();
        }

        /************************************************************************************/

        //it does not work for null substituting
        //[BeforeScenario]
        //public static void BeforeTestRun()
        //{
        //    Service.Instance.ValueRetrievers.Register(new NullValueRetriever("<null>"));
        //}

        //#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
        //[AfterTestRun(Order = 100_000)] //default is 10_000 (for injected hook)
        //public static void AfterAll()
        //{
        //    //Task.Delay(5000);
        //    //System.Threading.Thread.Sleep(5000);
        //    for (var i = 0; i < 50; i++)
        //        System.Threading.Thread.Sleep(100);
        //}
        //#pragma warning restore AsyncFixer01 // Unnecessary async/await usage

        [Given("the first number is (.*)")]
        public void GivenTheFirstNumberIs(int number)
        {
            //arrange (precondition) logic
            // For storing and retrieving scenario-specific data see https://go.specflow.org/doc-sharingdata
            // To use the multiline text or the table argument of the scenario,
            // additional string/Table parameters can be defined on the step definition
            // method. 

            //_scenarioCtx.Pending();
            _calculator.FirstNumber = number;
        }

        [Given("the second number is (.*)")]
        public void GivenTheSecondNumberIs(int number)
        {
            //arrange (precondition) logic

            //_scenarioCtx.Pending();
            _calculator.SecondNumber = number;
        }

        private int _result;

        [When("the two numbers are added")]
        public void WhenTheTwoNumbersAreAdded()
        {
            //action logic

            _result = _calculator.Add();
        }

        [When("the two numbers are substracted")]
        public void WhenTheTwoNumbersAreSubstracted()
        {
            //action logic

            _result = _calculator.Substract();
        }

        [Then("the int result should be (.*)")]
        public void ThenTheResultShouldBe(int result)
        {
            //assert (verification) logic

            _result.Should().Be(result);
        }
    }
}
