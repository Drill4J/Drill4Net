using System;
using TechTalk.SpecFlow;

namespace Drill4Net.Injection.SpecFlow
{
    /// <summary>
    /// It's just for the primer of the methods needed for the injections
    /// </summary>
    [Binding]
    internal class SpecFlowHooks
    {
        //group of tests
        [BeforeFeature(Order = 0)]
        public static void Drill4NetFeatureStarting(FeatureContext featureContext)
        {
            DemoTransmitter.DoCommand(0, featureContext.FeatureInfo.Title);
        }

        [AfterFeature(Order = 0)]
        public static void Drill4NetFeatureFinishing(FeatureContext featureContext)
        {
            DemoTransmitter.DoCommand(1, featureContext.FeatureInfo.Title);
        }

        //exactly separate tests
        [BeforeScenario(Order = 0)]
        public static void Drill4NetScenarioStarting(ScenarioContext scenarioContext)
        {
            DemoTransmitter.DoCommand(2, scenarioContext.ScenarioInfo.Title);
        }

        [AfterScenario(Order = 0)]
        public static void Drill4NetScenarioFinishing(ScenarioContext scenarioContext)
        {
            DemoTransmitter.DoCommand(3, scenarioContext.ScenarioInfo.Title);
        }
    }
}
