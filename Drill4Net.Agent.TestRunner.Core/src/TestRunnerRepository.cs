using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Drill4Net.Common;
using Drill4Net.Core.Repository;
using RestSharp;

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

        public async Task<List<BuildSummary>> GetBuildSummaries()
        {
            var url = GetUrl();
            var client = new RestClient(url);
            //client.Authenticator = new HttpBasicAuthenticator("username", "password");

            var request = new RestRequest(GetSummaryResource(), DataFormat.Json);
            //var a = client.Get(request);
            var summary = await client.GetAsync<List<BuildSummary>>(request);
            return summary;
        }
    }
}
