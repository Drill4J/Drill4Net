using System.Reflection;
using TechTalk.SpecFlow;
using Drill4Net.Agent.Abstract;
//using Drill4Net.Agent.Plugins.SpecFlow;

namespace Drill4Net.Injector.Plugins.SpecFlow
{
    /// <summary>
    /// It's just for the primier of the methods needed for the injections
    /// to the some target type, e.g. SpecFlowHooks
    /// </summary>
    [Binding]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1102:Make class static.", Justification = "<Pending>")]
    internal class SpecFlowHooks
    {
        //eventually, similar fields and methods should be generated

        //private static MethodInfo _featureMethInfo;
        private static MethodInfo _scenarioMethInfo;

        /*******************************************************************************************/

        //hooks in any test class intercept all tests in other classes
        //Need data: qualified name, display name, test group, result, assembly path

        //we need to start a single session at the start of the Runner run and close it after all tests are completed

        //let it will be replaced in method rather in cctor
        [BeforeTestRun]
        public static void VanchoTestsInit()
        {
            //the EXAMPLE!
            //hardcode or cfg?
            var plugPath = @"d:\Projects\EPM-D4J\Drill4Net\build\bin\Debug\Drill4Net.Agent.Plugins.SpecFlow\netstandard2.0\Drill4Net.Agent.Plugins.SpecFlow.dll";
            var asm = Assembly.LoadFrom(plugPath);
            var type = asm.GetType("Drill4Net.Agent.Plugins.SpecFlow.ContextHelper");
            //_featureMethInfo = type.GetMethod("GetFeatureContext");
            _scenarioMethInfo = type.GetMethod("GetScenarioContext");
            //
            DemoTransmitter.DoCommand((int)AgentCommandType.ASSEMBLY_TESTS_START, null);
        }

        [AfterTestRun]
        public static void VanchoTestsFinished()
        {
            DemoTransmitter.DoCommand((int)AgentCommandType.ASSEMBLY_TESTS_STOP, null);
        }

        //#region TEST
        ////groups (classes) of tests
        //[BeforeFeature(Order = 0)]
        //public static void Drill4NetFeatureStarting(FeatureContext featureContext)
        //{
        //    var data = GetContextData(_featureMethInfo, featureContext);
        //    DemoTransmitter.DoCommand((int)AgentCommandType.TEST_START, data);
        //    //DemoTransmitter.DoCommand((int)AgentCommandType.TEST_START, ContextHelper.GetFeatureContext(featureContext));
        //}

        //[AfterFeature(Order = 0)]
        //public static void Drill4NetFeatureFinished(FeatureContext featureContext)
        //{
        //    var data = GetContextData(_featureMethInfo, featureContext);
        //    DemoTransmitter.DoCommand((int)AgentCommandType.TEST_STOP, data);
        //    //DemoTransmitter.DoCommand((int)AgentCommandType.TEST_STOP, ContextHelper.GetFeatureContext(featureContext));
        //}
        //#endregion
        #region TEST_CASE
        //separate tests as cases of each test
        [BeforeScenario(Order = 0)]
        public static void Drill4NetScenarioStarting(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            var data = GetContextData(_scenarioMethInfo, featureContext, scenarioContext);
            DemoTransmitter.DoCommand((int)AgentCommandType.TEST_CASE_START, data);
            //DemoTransmitter.DoCommand((int)AgentCommandType.TEST_CASE_START, ContextHelper.GetScenarioContext(scenarioContext));
        }

        [AfterScenario(Order = 0)]
        public static void Drill4NetScenarioFinished(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            var data = GetContextData(_scenarioMethInfo, featureContext, scenarioContext);
            DemoTransmitter.DoCommand((int)AgentCommandType.TEST_CASE_STOP, data);
            //DemoTransmitter.DoCommand((int)AgentCommandType.TEST_CASE_STOP, ContextHelper.GetScenarioContext(scenarioContext));
        }
        #endregion

        private static string GetContextData(MethodInfo meth, FeatureContext featureCtx, ScenarioContext scenarioCtx)
        {
            return meth.Invoke(null,
                new object[]
                {
                    featureCtx,
                    scenarioCtx,
                    Assembly.GetExecutingAssembly().Location,
                }).ToString();
        }
    }
}
