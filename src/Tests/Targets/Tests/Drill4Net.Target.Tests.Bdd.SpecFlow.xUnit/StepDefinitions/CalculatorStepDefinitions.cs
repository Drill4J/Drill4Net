using FluentAssertions;
using TechTalk.SpecFlow;
using Drill4Net.Target.Testers.Common;

namespace Drill4Net.Target.Tests.Bdd.SpecFlow.xUnit.StepDefinitions
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
