using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Standard;
using Drill4Net.Core.Repository;
using Drill4Net.Admin.Requester;
using Drill4Net.Agent.Abstract;
using Newtonsoft.Json;

namespace Drill4Net.Agent.TestRunner.Core
{
    public class TestRunnerRepository : ConfiguredRepository<TestRunnerOptions, BaseOptionsHelper<TestRunnerOptions>>
    {

        private readonly AdminRequester _requester;
        private readonly Logger _logger;

        /********************************************************************************/

        public TestRunnerRepository(): base(CoreConstants.SUBSYSTEM_AGENT_TEST_RUNNER, string.Empty)
        {
            _logger = new TypedLogger<TestRunnerRepository>(Subsystem);
            //TODO: WRONG!!! we need get the Target version by Options.FilePath (when Admin sode will get the Target version, not Agent's one)
            var version = FileUtils.GetProductVersion(typeof(TestRunnerRepository));
            _requester = new(Options.Url, Options.Target, version);
        }

        /********************************************************************************/

        internal StandardAgent GetAgent()
        {
            var agentCfgPath = Path.Combine(FileUtils.EntryDir, CoreConstants.CONFIG_NAME_DEFAULT);
            var helper = new TreeRepositoryHelper(Subsystem);
            var treePath = helper.CalculateTreeFilePath(Path.GetDirectoryName(Options.FilePath));
            var agentRep = new StandardAgentRepository(agentCfgPath, treePath);
            return new StandardAgent(agentRep);
        }

        public async Task<(RunningType runType, List<string> tests)> GetRunToTests()
        {
            var tests = new List<string>();
            var runType = GetRunningType();
            _logger.Debug($"Real running type: {runType}");

            var isFake = Options.Debug is { Disabled: false, IsFake: true };
            _logger.Info($"Fake mode: {isFake}");
            if (isFake)
            {
                runType = RunningType.Certain;
                _logger.Debug($"Fake running type: {runType}");
            }

            if (runType == RunningType.Certain)
            {
                var run = await (!isFake ? _requester.GetTestToRun() : GetFakeTestToRun())
                    .ConfigureAwait(false);
                foreach (var type in run.ByType.Keys)
                {
                    var testByType = run.ByType[type];
                    foreach (var t2r in testByType)
                    {
                        //Name must be equal to QualifiedName... or to get exactly the QualifiedName from metadata
                        var name = t2r.Name; //it is DisplayName, not QualifiedName
                        var meta = t2r.Metadata; //TODO: use info about executing file!
                        string qName = null;
                        if(meta.ContainsKey(AgentConstants.KEY_TESTCASE_CONTEXT))
                            qName = GetTestCaseContext(meta[AgentConstants.KEY_TESTCASE_CONTEXT])?.QualifiedName;
                        if (string.IsNullOrWhiteSpace(qName))
                            qName = TestContextHelper.GetQualifiedName(name);
                        tests.Add(qName);
                    }
                }
            }
            return (runType, tests);
        }

        protected TestCaseContext GetTestCaseContext(string str)
        {
            return JsonConvert.DeserializeObject<TestCaseContext>(str);
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

        internal virtual RunningType GetRunningType()
        {
            //TODO: add error handling
            List<BuildSummary> summary = _requester.GetBuildSummaries();
            var count = (summary?.Count) ?? 0;
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
                if (test2Run.Count > 0)
                    return RunningType.Certain;

                //hmm...
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
    }
}
