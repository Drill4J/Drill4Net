using FluentAssertions;
using TechTalk.SpecFlow;
using Drill4Net.Target.Testers.Common;
using System.Threading;
using System.Reflection;
using System;
using System.Runtime.Serialization;

namespace Drill4Net.Target.Tests.Bdd.SpecFlow.StepDefinitions
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

        //DON'T REMOVE THIS EVEN IF IT IS COMMENTED
        [AfterScenario(Order = 0)]
        public static void DebugScenarioFinished(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            var feature = $"{featureContext.FeatureInfo.FolderPath}/{featureContext.FeatureInfo.Title}";
            var scenario = scenarioContext.ScenarioInfo.Title;
            var testStatus = scenarioContext.ScenarioExecutionStatus;
            var testError = scenarioContext.TestError;

            var ec = Thread.CurrentThread.ExecutionContext;

            //var ec = ExecutionContext.Capture();
            var sc = SynchronizationContext.Current;
            //var newSc = new SynchronizationContext();

            var execCtx = GetContext();
        }

        /************************************************************************************/

        private const string CONTEXT_UNKNOWN = "unknown";
        private static string GetContext()
        {
            //try
            //{
            //    //try load old CallContext type - for NetFx successfully
            //    var testCtx = LogicalContextManager.GetNUnitTestContext();
            //    if (testCtx != null)
            //        return GetContextId(testCtx);
            //}
            //catch { } //it's normal under the NetCore

            //...and for NetCore tests NUnit uses AsyncLocal.
            //var lstFlds = typeof(ExecutionContext).GetFields();
            var lstFld = Array.Find(typeof(ExecutionContext)
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance), a => a.Name == "m_localValues");
            if (lstFld != null)
            {
                var lstFldVal = lstFld.GetValue(Thread.CurrentThread.ExecutionContext);
                if (lstFldVal != null)
                {
                    //don't cache... (TODO: check it again)
                    var typeValMap = Type.GetType("System.Threading.AsyncLocalValueMap+ThreeElementAsyncLocalValueMap");
                    var ctxFld = Array.Find(typeValMap
                        .GetFields(BindingFlags.NonPublic | BindingFlags.Instance), a => a.Name == "_value3");
                    if (ctxFld != null)
                    {
                        //if (_execIdToTestId == null)
                        //    return CONTEXT_UNKNOWN;

                        //This defines the logical execution path of function callers regardless
                        //of whether threads are created in async/await or Parallel.For
                        //It doesn't work very well on its own, at least not for everyone's version 
                        //of the framework.
                        var execId = Thread.CurrentThread.ExecutionContext.GetHashCode();

                        try
                        {
                            var testCtx = ctxFld.GetValue(lstFldVal); // as TestExecutionContext;

                            //var id = GetContextId(testCtx);
                            //if (!_execIdToTestId.ContainsKey(execId))
                            //    _execIdToTestId.Add(execId, id);

                            //return id;
                            return "???";
                        }
                        catch
                        {
                            //here we will be, for example, for object's Finalizers
                            typeValMap = Type.GetType("System.Threading.AsyncLocalValueMap+OneElementAsyncLocalValueMap");
                            ctxFld = Array.Find(typeValMap
                                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance), a => a.Name == "_value1");
                            //no context info about concrete test          
                            var testCtx = ctxFld.GetValue(lstFldVal); // as TestExecutionContext;
                            //var testOutput = GetContextOutput(testCtx);

                            //return _execIdToTestId.ContainsKey(execId) ? _execIdToTestId[execId] : CONTEXT_UNKNOWN;
                            return "???";
                        }
                    }
                }
            }
            return CONTEXT_UNKNOWN;
        }

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
            var ec = Thread.CurrentThread.ExecutionContext;

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
