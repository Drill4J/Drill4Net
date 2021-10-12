using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
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

        public TestRunnerRepository(): base(string.Empty, CoreConstants.SUBSYSTEM_AGENT_TEST_RUNNER)
        {
            _logger = new TypedLogger<TestRunnerRepository>(Subsystem);

            var url = GetUrl();
            _client = new RestClient(url);
            //client.Authenticator = new HttpBasicAuthenticator("username", "password");
        }

        /********************************************************************************/

        public async Task<List<BuildSummary>> GetBuildSummaries()
        {
            var request = new RestRequest(GetSummaryResource(), DataFormat.Json);
            //var a = client.Get(request);
            var summary = await _client.GetAsync<List<BuildSummary>>(request)
                .ConfigureAwait(false);
            return summary;
        }

        internal async Task<(RunningType runType, List<string> tests)> GetRunToTests()
        {
            var tests = new List<string>();
            var runType = await GetRunningType().ConfigureAwait(false);
            if (runType != RunningType.Nothing)
            {
                ////FAKE tests - TODO: real getting from Drill Admin by WS !!!
                //tests.Add("PublishersArray");
                //tests.Add("BookStateUpdateFails");
                //tests.Add("SortByDealDates");

                var run = await GetTestToRun();
                foreach (var type in run.ByType.Keys)
                {
                    var testByType = run.ByType[type];
                    foreach (var t2r in testByType)
                    {
                        var name = t2r.Name;
                        var meta = t2r.Metadata;
                    }
                }
                
            }
            return (runType, tests);
        }

        internal async virtual Task<TestToRunResponse> GetTestToRun()
        {
            ////https://kb.epam.com/display/EPMDJ/Code+Coverage+plugin+endpoints
            //var request = new RestRequest(GetTest2RunResource(), DataFormat.Json);
            ////var a = client.Get(request);
            //var run = await _client.GetAsync<TestToRunResponse>(request)
            //     .ConfigureAwait(false);
            //return run;

            //FAKE TEST !!!
            //these tests we have to run
            var forRun = @"
{
    ""byType"":{
    ""AUTO"":[
        {
        ""name"":""PublishersArray"",
        ""metadata"":{
                 ""AssemblyPath"":""Ipreo.Csp.IaDeal.Api.Bdd.Tests.dll"",
                 ""QualifiedName"":""PublishersArray"",
                }
        },
        {
        ""name"":""BookStateUpdateFails"",
        ""metadata"":{
                ""AssemblyPath"":""Ipreo.Csp.IaDeal.Api.Bdd.Tests.dll"",
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
            var run = System.Text.Json.JsonSerializer.Deserialize<TestToRunResponse>(forRun, opts);

            return run;
        }

        internal async virtual Task<RunningType> GetRunningType()
        {
            //TODO: add error handling
            List<BuildSummary> summary = await GetBuildSummaries().ConfigureAwait(false);
            _logger.Debug($"Builds: {summary.Count}");
            //
            var runType = RunningType.All;
            TestToRunSummaryInfo test2Run = null;
            if (summary.Count > 0) //some builds exists
            {
                summary = summary.OrderByDescending(a => a.DetectedAt).ToList();
                var actual = summary[0];
                test2Run = actual?.Summary?.TestsToRun;
                if (test2Run == null)
                    throw new Exception("No object of test2Run");

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

        public string GetTest2RunResource()
        {
            return $"api/agents/{Options.Target}/plugins/test2code/data/tests-to-run";
        }
    }
}
