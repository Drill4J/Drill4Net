using System.Reflection;

namespace Drill4Net.Injection.SpecFlow
{
    /// <summary>
    /// Fake SpecFlowHooks for Cecilifier https://cecilifier.me/ - translating service from C# to CIL
    /// </summary>
    internal class SpecFlowHooksCecilifier
    {
        //eventually, similar fields and methods should be generated

        //private static MethodInfo _featureMethInfo;
        private static MethodInfo _scenarioMethInfo;

        /*******************************************************************************************/

        static SpecFlowHooksCecilifier()
        {
            //the EXAMPLE!
            //hardcode or cfg?
            var plugPath = @"d:\Projects\EPM-D4J\Drill4Net\build\bin\Debug\Drill4Net.Agent.Transmitter.SpecFlow\netstandard2.0\Drill4Net.Agent.Transmitter.SpecFlow.dll";
            var asm = Assembly.LoadFrom(plugPath);
            var type = asm.GetType("Drill4Net.Agent.Transmitter.SpecFlow.ContextHelper");
            //_featureMethInfo = type.GetMethod("GetFeatureContext");
            _scenarioMethInfo = type.GetMethod("GetScenarioContext");
        }

        /*******************************************************************************************/

        #region TESTS_RUN
        public static void VanchoTestsStarting()
        {
            DemoTransmitter2.DoCommand((int)AgentCommandType2.CLASS_TESTS_START, null);
        }

        public static void VanchoTestsFinished()
        {
            DemoTransmitter2.DoCommand((int)AgentCommandType2.CLASS_TESTS_STOP, null);
        }
        #endregion
        #region TEST_CASE
        public static void Drill4NetScenarioStarting(object featureContext, object scenarioContext)
        {
            var data = GetContextData(_scenarioMethInfo, featureContext, scenarioContext);
            DemoTransmitter2.DoCommand((int)AgentCommandType2.TEST_CASE_START, data);
        }

        public static void Drill4NetScenarioFinished(object featureContext, object scenarioContext)
        {
            var data = GetContextData(_scenarioMethInfo, featureContext, scenarioContext);
            DemoTransmitter2.DoCommand((int)AgentCommandType2.TEST_CASE_STOP, data);
        }
        #endregion

        private static string GetContextData(MethodInfo meth, object featureCtx, object scenarioCtx)
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

    internal enum AgentCommandType2
    {
        CLASS_TESTS_START = 0,
        CLASS_TESTS_STOP = 1,

        TEST_START = 2,
        TEST_STOP = 3,

        TEST_CASE_START = 4,
        TEST_CASE_STOP = 5,
    }

    internal class DemoTransmitter2
    {
        internal static void DoCommand(int command, string data) { }
    }
}
