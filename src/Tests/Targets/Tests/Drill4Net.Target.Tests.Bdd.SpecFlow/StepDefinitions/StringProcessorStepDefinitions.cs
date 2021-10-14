using System;
using FluentAssertions;
using TechTalk.SpecFlow;
using Drill4Net.Target.Testers.Common;

namespace Drill4Net.Target.Tests.Bdd.SpecFlow.StepDefinitions
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
            _inStr = inStr;
        }

        [When("Uppercase it")]
        public void WhenAction()
        {
            _res = _processor.Uppercase(_inStr);
        }

        [Then("the string result should be (.*)")]
        public void ThenOutcome(string result)
        {
            _res.Should().Be(result);
        }
    }
}
