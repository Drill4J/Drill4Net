using TechTalk.SpecFlow;

namespace Drill4Net.Injection.SpecFlow
{
    /// <summary>
    /// It's just for the primer of the methods needed for the injections
    /// </summary>
    [Binding]
    internal class SpecFlowHooks
    {
        //groups (classes) of tests
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

        //separate tests as cases of each test
        [BeforeScenario(Order = 0)]
        public static void Drill4NetScenarioStarting(ScenarioContext scenarioContext)
        {
            // EXAMPLE HOW GET THE CASE TEST FULLNAME

            //Request for never populated field
            //Sort by deal dates(scenarioDescription: "Asc sorting DealCreatedDate", sortField: "DealCreatedDate", sortDirection: "Ascending", versionsReturned: "5,6,4", exampleTags: [])
            var sc = scenarioContext.ScenarioInfo;
            var title = sc.Title;
            var args = sc.Arguments;
            var tags = sc.Tags;
            var isParams = args.Count > 0 || tags.Length > 0;
            if (isParams)
                title += "(";
            //
            if (args.Count > 0)
            {
                var argsS = string.Empty;
                foreach (System.Collections.DictionaryEntry entry in args)
                {
                    //paramName
                    var key = entry.Key.ToString().Replace(" ", null);
                    char[] a = key.ToCharArray();
                    a[0] = char.ToLower(a[0]);
                    key = new string(a);

                    argsS += $"{key}: \"{entry.Value}\", ";
                }
                title += argsS;
            }
            //
            if (isParams)
                title += "exampleTags: [";
            if (tags.Length > 0)
            {
                foreach (var tag in tags)
                    title += tag + ", ";
                title = title[0..^2];
            }
            if (isParams)
                title += "])";
            //
            DemoTransmitter.DoCommand(2, title);
        }

        [AfterScenario(Order = 0)]
        public static void Drill4NetScenarioFinishing(ScenarioContext scenarioContext)
        {
            DemoTransmitter.DoCommand(3, scenarioContext.ScenarioInfo.Title);
        }
    }
}
