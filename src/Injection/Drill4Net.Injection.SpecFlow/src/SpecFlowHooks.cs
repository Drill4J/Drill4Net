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

        public static void ForCecilifier()
        {
            var title = "TITLE";
            System.Collections.Specialized.IOrderedDictionary args = new System.Collections.Specialized.OrderedDictionary();
            var tags = System.Array.Empty<string>();
            var isParams = args.Count > 0 || tags.Length > 0;
            if (isParams)
                title += "(";
            //
            if (args.Count > 0)
            {
                var argsS = string.Empty;
                for (var i = 0; i < args.Count; i++)
                {
                    var entry = (System.Collections.DictionaryEntry)args[i];

                    //paramName
                    var key = entry.Key.ToString().Replace(" ", null);
                    char[] a = key.ToCharArray();
                    a[0] = char.ToLower(a[0]);
                    key = new string(a);

                    argsS += $"{key}: \"{entry.Value}\", ";
                }
                //foreach (System.Collections.DictionaryEntry entry in args)
                //{
                //    //paramName
                //    var key = entry.Key.ToString().Replace(" ", null);
                //    char[] a = key.ToCharArray();
                //    a[0] = char.ToLower(a[0]);
                //    key = new string(a);

                //    argsS += $"{key}: \"{entry.Value}\", ";
                //}
                title += argsS;
            }
            //
            if (isParams)
                title += "exampleTags: [";
            if (tags.Length > 0)
            {
                foreach (var tag in tags)
                    title += tag + ", ";
                //title = title[0..^2];
                title = title.Substring(0, title.Length - 2);
            }
            if (isParams)
                title += "])";
        }
    }
}
