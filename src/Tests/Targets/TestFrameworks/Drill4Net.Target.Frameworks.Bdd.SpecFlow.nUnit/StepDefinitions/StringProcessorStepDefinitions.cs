using FluentAssertions;
using TechTalk.SpecFlow;
using Drill4Net.Target.Frameworks.Common;

namespace Drill4Net.Target.Frameworks.Bdd.SpecFlow.nUnit.StepDefinitions
{
    [Binding]
    public class StringProcessorStepDefinitions
    {
        private string _inStr;
        private string _res;
        private readonly StringProcessor _processor = new();

        /***********************************************************************/

        [Given("Input string is (.*)")]
        public void GivenContext(string inStr)
        {
            _inStr = PreprocessValue(inStr);
        }

        [When("Uppercase it")]
        public void WhenAction()
        {
            _res = _processor.Uppercase(_inStr);
        }

        [Then("the string result should be (.*)")]
        public void ThenOutcome(string result)
        {
            _res.Should().Be(PreprocessValue(result));
        }

        private string PreprocessValue(string val)
        {
            return val == "null" ? null : val;
        }
    }
}
