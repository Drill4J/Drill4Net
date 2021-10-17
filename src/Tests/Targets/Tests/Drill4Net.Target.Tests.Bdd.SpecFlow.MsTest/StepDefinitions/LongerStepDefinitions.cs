using System;
using FluentAssertions;
using TechTalk.SpecFlow;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Drill4Net.Target.Testers.Common;

//https://docs.specflow.org/projects/specflow/en/latest/Execution/Parallel-Execution.html
//https://stackoverflow.com/questions/3917060/how-to-run-unit-tests-mstest-in-parallel
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

#if NETFRAMEWORK
            System.Runtime.Remoting.Messaging.CallContext.LogicalSetData("TestCase", key);
#endif
            //https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Threading/ExecutionContext.cs
            var ec = Thread.CurrentThread.ExecutionContext;
            var sc = SynchronizationContext.Current;
            //var newSc = new SynchronizationContext();

            var execCtx = GetContext();
        }

        [AfterScenario(Order = 0)]
        public static void DebugScenarioFinished(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            var feature = $"{featureContext.FeatureInfo.FolderPath}/{featureContext.FeatureInfo.Title}";
            var scenario = scenarioContext.ScenarioInfo.Title;
            var testStatus = scenarioContext.ScenarioExecutionStatus;
            var testError = scenarioContext.TestError;

#if NETFRAMEWORK
            //will be empty (because, infortunately, it is another context)
            var data = System.Runtime.Remoting.Messaging.CallContext.LogicalGetData("TestCase");
            Debug.WriteLine($"*** Data of context: [{data}]");
#endif

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

        [When("do long work for (.*)")]
        public void WaitTimeout(int timeout)
        {
            var ec = Thread.CurrentThread.ExecutionContext;

            _longer.DoLongWork(timeout);
        }

        [When("do default long work")]
        public void DoDefaultAction()
        {
            var ec = Thread.CurrentThread.ExecutionContext;

            _longer.DoLongWork();
        }
    }
}
