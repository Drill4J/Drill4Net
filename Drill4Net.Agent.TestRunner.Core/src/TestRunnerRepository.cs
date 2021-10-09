using System;
using System.Linq;
using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.TestRunner.Core
{
    public class TestRunnerRepository : ConfiguredRepository<TestRunnerOptions, BaseOptionsHelper<TestRunnerOptions>>
    {
        public TestRunnerRepository(): base(string.Empty, CoreConstants.SUBSYSTEM_AGENT_TEST_RUNNER)
        {
        }

        /********************************************************************************/

        public string GetUrl()
        {
            var url = Options.Url;
            if (!url.StartsWith("http"))
                url = "http://" + url; //TODO: check for https
            return url; // $"{url}/api/agents/{Options.Target}/plugins/test2code/builds/summary";
        }

        public string GetSummaryResource()
        {
            return $"api/agents/{Options.Target}/plugins/test2code/builds/summary";
        }
    }
}
