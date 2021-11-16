using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Standard;
using Drill4Net.Core.Repository;
using Drill4Net.Admin.Requester;

namespace Drill4Net.Agent.TestRunner.Core
{
    public class TestRunnerRepository : ConfiguredRepository<TestRunnerOptions, BaseOptionsHelper<TestRunnerOptions>>
    {
        //TODO: more abstract type (but Agent shouldn't connect immedeately after its creating)
        private readonly StandardAgentRepository _agentRep;

        private readonly AdminRequester _requester;
        private readonly Logger _logger;

        /********************************************************************************/

        public TestRunnerRepository(): base(CoreConstants.SUBSYSTEM_AGENT_TEST_RUNNER, string.Empty)
        {
            _logger = new TypedLogger<TestRunnerRepository>(Subsystem);
            if (string.IsNullOrWhiteSpace(Options.Directory))
                throw new Exception("Directory for tests is empty in config");
            _agentRep = CreateAgentRepository();
            _requester = new(_agentRep.Options.Admin.Url, _agentRep.TargetName, _agentRep.TargetVersion);
        }

        /********************************************************************************/

        internal StandardAgentRepository CreateAgentRepository()
        {
            //we need to give the concrete path to the agent config because also the others  are located here
            var agentCfgPath = Path.Combine(FileUtils.EntryDir, CoreConstants.CONFIG_NAME_DEFAULT);

            var helper = new TreeRepositoryHelper(Subsystem);
            var treePath = helper.CalculateTreeFilePath(Options.Directory);
            return new StandardAgentRepository(agentCfgPath, treePath);
        }

        internal StandardAgent CreateAgent()
        {
            StandardAgentCCtorParameters.SkipCreatingSingleton = true;
            return new StandardAgent(_agentRep);
        }

        internal async Task<RunInfo> GetRunInfo()
        {
            var isFake = Options.Debug is { Disabled: false, IsFake: true };
            _logger.Info($"Fake mode: {isFake}");

            RunningType runningType = await GetRunningType(isFake).ConfigureAwait(false);
            _logger.Debug($"Running type: {runningType}");

            var res = new RunInfo { RunType = runningType };

            #region Certain
            if (runningType == RunningType.Certain)
            {
                // get tests to run from Admin
                var run = await GetTestToRun(isFake)
                    .ConfigureAwait(false);
                if (run.ByType == null) //it is error here
                {
                    _logger.Error("No tests");
                    return res;
                }

                if (!run.ByType.ContainsKey(AgentConstants.TEST_AUTO))
                {
                    res.RunType = RunningType.Nothing;
                    return res;
                }

                // adapt tests to run in CLI
                foreach (var t2r in run.ByType[AgentConstants.TEST_AUTO])
                {
                    //Name must be equal to QualifiedName... or to get exactly the QualifiedName from metadata
                    var name = t2r.Name; //it is DisplayName, not QualifiedName
                    var metadata = t2r.Metadata.data; //TODO: use info about executing file!
                    string qName = null;
                    string asmName = null;
                    bool mustSeq = true;
                    if (metadata.ContainsKey(AgentConstants.KEY_TESTCASE_CONTEXT))
                    {
                        var ctx = GetTestCaseContext(metadata[AgentConstants.KEY_TESTCASE_CONTEXT]);
                        if (ctx != null)
                        {
                            asmName = Path.GetFileName(ctx.AssemblyPath);
                            qName = ctx.QualifiedName;
                            if (ctx.Engine != null)
                                mustSeq = ctx.Engine.MustSequential;
                        }
                    }

                    //test qualified name
                    if (string.IsNullOrWhiteSpace(qName))
                        qName = TestContextHelper.GetQualifiedName(name);

                    // assembly path
                    if (string.IsNullOrWhiteSpace(asmName))
                    {
                        _logger.Error($"Unknowm test assembly for test {qName}");
                        continue;
                    }

                    // insert test info
                    RunAssemblyInfo info;
                    if (res.AssemblyInfos.ContainsKey(asmName))
                    {
                        info = res.AssemblyInfos[asmName];
                    }
                    else
                    {
                        info = new()
                        {
                            AssemblyName = asmName,
                            MustSequential = mustSeq,
                            Tests = new(),
                        };
                        res.AssemblyInfos.Add(asmName, info);
                    }
                    info.Tests.Add(qName);
                }
                return res;
            }
            #endregion
            #region All
            if (res.RunType == RunningType.All)
            {
                var allRun = new RunAssemblyInfo
                {
                    AssemblyName = Options.DefaultAssemblyName,
                    MustSequential = Options.DefaultParallelRestrict,
                };
                res.AssemblyInfos.Add(allRun.AssemblyName, allRun);
            }
            #endregion
            return res;
        }

        internal async Task<TestToRunResponse> GetTestToRun(bool isFake)
        {
            TestToRunResponse run = null;
            for (var i = 0; i < 5; i++) //guanito, but Drill REST can be starnge
            {
                run = await(!isFake ? _requester.GetTestToRun() : GetFakeTestToRun())
                    .ConfigureAwait(false);
                if (run.ByType != null)
                    break;
                await Task.Delay(1000).ConfigureAwait(false);
            }
            return run;
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

        internal async virtual Task<RunningType> GetRunningType(bool isFake)
        {
            RunningType runningType;
            if (isFake)
            {
                runningType = RunningType.Certain;
            }
            else
            {
                //TODO: add error handling
                //List<BuildSummary> summary = await _requester.GetBuildSummaries();
                await _agentRep.RetrieveTargetBuilds();
                var summary = _agentRep.Builds;
                var count = (summary?.Count) ?? 0;
                _logger.Debug($"Builds: {count}");
                //
                runningType = RunningType.All;
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
                        runningType = test2runCnt == 0 ?
                            RunningType.Nothing :
                            RunningType.Certain;
                    }
                }
            }
            return runningType;
        }
    }
}
