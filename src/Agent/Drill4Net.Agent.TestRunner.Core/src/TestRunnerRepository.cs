using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.TestRunner.Core
{
    public class TestRunnerRepository : ConfiguredRepository<TestRunnerOptions, BaseOptionsHelper<TestRunnerOptions>>
    {
        private readonly RestClient _client;
        private readonly Logger _logger;

        /********************************************************************************/

        public TestRunnerRepository(): base(CoreConstants.SUBSYSTEM_AGENT_TEST_RUNNER, string.Empty)
        {
            _logger = new TypedLogger<TestRunnerRepository>(Subsystem);

            var url = GetUrl();
            _client = new RestClient(url);
            //client.Authenticator = new HttpBasicAuthenticator("username", "password");
        }

        /********************************************************************************/

        public async Task<(RunningType runType, List<string> tests)> GetRunToTests()
        {
            var tests = new List<string>();
            var runType = GetRunningType();
            _logger.Debug($"Running type: {runType}");

            var isFake = true; //TEST !!!!
            if (isFake)
                runType = RunningType.Certain;

            if (runType == RunningType.Certain)
            {
                var run = await (!isFake ? GetTestToRun() : GetFakeTestToRun())
                    .ConfigureAwait(false);
                foreach (var type in run.ByType.Keys)
                {
                    var testByType = run.ByType[type];
                    foreach (var t2r in testByType)
                    {
                        //Name must be equal to QualifiedName... or to get exactly the QualifiedName from metadata
                        var name = t2r.Name;
                        var meta = t2r.Metadata;
                        tests.Add(name);
                    }
                }
            }
            return (runType, tests);
        }

        #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        internal async virtual Task<TestToRunResponse> GetFakeTestToRun()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            //FAKE TEST !!!
            //these tests we have to run
            const string forRun = @"
            {
                ""byType"":{
                ""AUTO"":[
                    {
                    ""name"":""PublishersArray"",
                    ""metadata"":{
                             ""AssemblyPath"":""d:\\Projects\\IHS-bdd.Injected\\Ipreo.Csp.IaDeal.Api.Bdd.Tests.dll"",
                             ""QualifiedName"":""PublishersArray"",
                            }
                    },
                    {
                    ""name"":""BookStateUpdateFails"",
                    ""metadata"":{
                            ""AssemblyPath"":""d:\\Projects\\IHS-bdd.Injected\\Ipreo.Csp.IaDeal.Api.Bdd.Tests.dll"",
                            ""QualifiedName"":""BookStateUpdateFails"",
                           }
                     }
                ]
                },
                ""totalCount"":2
            }";
            var opts = new System.Text.Json.JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
            };
            return System.Text.Json.JsonSerializer.Deserialize<TestToRunResponse>(forRun, opts);
        }

        internal async virtual Task<TestToRunResponse> GetTestToRun()
        {
            //https://kb.epam.com/display/EPMDJ/Code+Coverage+plugin+endpoints
            var request = new RestRequest(GetTest2RunResource(), DataFormat.Json);
            //var a = client.Get(request);
            var run = await _client.GetAsync<TestToRunResponse>(request)
                 .ConfigureAwait(false);
            return run;
        }

        internal virtual RunningType GetRunningType()
        {
            //TODO: add error handling
            List<BuildSummary> summary = GetBuildSummaries();
            var count = summary == null ? 0 : summary.Count;
            _logger.Debug($"Builds: {count}");
            //
            var runType = RunningType.All;
            TestToRunSummaryInfo test2Run = null;
            if (count > 0) //some builds exists
            {
                summary = summary.OrderByDescending(a => a.DetectedAt).ToList();
                var actual = summary[0];
                test2Run = actual?.Summary?.TestsToRun;
                if (test2Run == null)
                    return RunningType.All;

                var testCnt = actual.Summary.Tests.Count;
                var test2runCnt = test2Run.Count;
                _logger.Debug($"Total tests: {testCnt}, tests to run: {test2runCnt}");
                //
                if (testCnt > 0)
                {
                    //tests exists but no test to run (no difference between builds)
                    runType = test2runCnt == 0 ?
                        RunningType.Nothing :
                        RunningType.Certain;
                }
            }
            _logger.Debug($"Running type: {runType}");
            return runType;
        }

        internal virtual List<BuildSummary> GetBuildSummaries()
        {
            //http://localhost:8090/api/agents/IHS-bdd/plugins/test2code/builds/summary

            var request = new RestRequest(GetSummaryResource(), Method.GET, DataFormat.Json);
            var a = _client.Get(request);
            if (a.StatusCode != System.Net.HttpStatusCode.OK)
                return null;
            var summary = JsonConvert.DeserializeObject<List<BuildSummary>>(a.Content);
            //var summary = await _client.GetAsync<List<BuildSummary>>(request) //it is failed on empty member (Summary)
            //    .ConfigureAwait(false);
            return summary;
        }

        public string GetUrl()
        {
            //{url}/api/agents/{Id}/plugins/test2code/builds/summary
            var url = Options.Url;
            if (!url.StartsWith("http"))
                url = "http://" + url; //TODO: check for https
            return url;
        }

        public string GetSummaryResource()
        {
            return $"api/agents/{Options.Target}/plugins/test2code/builds/summary";
        }

        public string GetTest2RunResource()
        {
            return $"api/agents/{Options.Target}/plugins/test2code/data/tests-to-run";
        }
    }
}
