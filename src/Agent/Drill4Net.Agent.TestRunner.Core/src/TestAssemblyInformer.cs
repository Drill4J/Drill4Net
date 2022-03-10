using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Repository;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Standard;
using Drill4Net.Admin.Requester;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// Informer about test needed to run for specified directory and assembly
    /// </summary>
    public class TestAssemblyInformer
    {
        public string TargetName { get; }

        public string AssemblyPath => FileUtils.GetFullPath(Path.Combine(_dirOptions.Directory, _asmOptions.DefaultAssemblyName));

        private readonly RunDirectoryOptions _dirOptions;
        private readonly RunAssemblyOptions _asmOptions;
        private readonly TestRunnerDebugOptions _dbgOpts;

        //TODO: more abstract type (but Agent shouldn't connect immediately after its creating)
        private readonly StandardAgentRepository _agentRep;
        private readonly StandardAgent _agent;

        private static TestToRunResponse _t2run;
        private readonly AdminRequester _adminRequester;
        private const string Subsystem = CoreConstants.SUBSYSTEM_TEST_RUNNER;
        private readonly ManualResetEventSlim _initBlocker = new(false);
        private readonly Logger _logger;

        /***************************************************************************/

        public TestAssemblyInformer(RunDirectoryOptions dirOptions, RunAssemblyOptions asmOptions, TestRunnerDebugOptions dbgOpts = null)
        {
            _dirOptions = dirOptions ?? throw new Exception("Test assembly directory's options is empty");
            _asmOptions = asmOptions ?? throw new Exception("Test assembly's options is empty");
            if (string.IsNullOrWhiteSpace(asmOptions.DefaultAssemblyName))
                throw new Exception("Assembly name is empty");
            if (!File.Exists(AssemblyPath))
                throw new Exception("Test assembly does not exist");
            //
            _dbgOpts = dbgOpts;
            _logger = new TypedLogger<TestAssemblyInformer>(Subsystem);
            _agentRep = CreateAgentRepository();
            TargetName = _agentRep.TargetName;
            _logger.Extras.Add("Target", TargetName);
            _logger.RefreshExtrasInfo();
            _adminRequester = new(CoreConstants.SUBSYSTEM_TEST_RUNNER, _agentRep.Options.Admin.Url,
                _agentRep.TargetName, _agentRep.TargetVersion);

            // agent
            _logger.Debug("Preparing the agent...");
            _agent = CreateAgent();
            _agent.Initialized += AgentInitialized;

            _logger.Debug("Wait for the agent's initializing...");
            _initBlocker.Wait(); //waiting for agent's init
        }

        /***************************************************************************/

        internal StandardAgentRepository CreateAgentRepository()
        {
            var dir = _dirOptions.Directory;

            //we need to give the concrete path to the agent config from target's directory
            var optsHelper = new BaseOptionsHelper(Subsystem);
            var agentCfgPath = optsHelper.GetActualConfigPath(dir);

            var helper = new TreeRepositoryHelper(Subsystem);
            var treePath = helper.CalculateTreeFilePath(dir);
            return new StandardAgentRepository(agentCfgPath, treePath);
        }

        internal StandardAgent CreateAgent()
        {
            AgentInitParameters.SkipCreatingSingleton = true;
            return new StandardAgent(_agentRep);
        }

        private void AgentInitialized()
        {
            _initBlocker.Set();
        }

        internal async Task<DirectoryRunInfo> GetRunInfo(RunningType overridenType = RunningType.Unknown)
        {
            var isFake = _dbgOpts is { Disabled: false, IsFake: true };
            _logger.Debug($"Fake mode: {isFake}");

            #region Calc running type
            var assocCtxs = await GetAssociatedTests();
            var assocTests = assocCtxs.Select(a => a.DisplayName);
            var asmTests = await GetAssemblyTests();
            var shareTests = asmTests.Intersect(assocTests);
            var newTests = asmTests.Except(shareTests) //we have only DisplayNames
                .Select(a => TestContextHelper.GetQualifiedName(a))
                .ToList();
            var delTests = assocTests.Except(shareTests) //we have full test case contexts
                .Select(a => TestContextHelper.GetQualifiedName(a))
                .ToHashSet();
            _logger.Info($"New tests: {newTests.Count}, deleted tests: {delTests.Count}");

            var runningType = overridenType == RunningType.Unknown ?
                await GetRunningTypeByService(isFake).ConfigureAwait(false):
                overridenType;

            if (runningType == RunningType.Nothing && newTests.Count > 0)
                runningType = RunningType.Certain;
            
            _logger.Info($"Running type: {runningType}");
            #endregion

            var dirInfo = new DirectoryRunInfo
            {
                Target = TargetName,
                RunType = runningType,
                DirectoryOptions = _dirOptions,
                AssemblyOptions = _asmOptions,
            };

            #region Certain
            if (runningType == RunningType.Certain)
            {
                // get tests to run from Admin
                if(_t2run == null)
                    _t2run = await GetTestsToRun(isFake).ConfigureAwait(false);

                var seqDict = new Dictionary<string, bool>();
                if (newTests.Count == 0)
                {
                    if (_t2run.ByType == null) //it is error here
                    {
                        _logger.Error("No tests");
                        return dirInfo;
                    }

                    if (!_t2run.ByType.ContainsKey(AgentConstants.TEST_AUTO))
                    {
                        dirInfo.RunType = RunningType.Nothing;
                        return dirInfo;
                    }
                    //
                    var nameHash = new HashSet<string>();

                    foreach (var t2r in _t2run.ByType[AgentConstants.TEST_AUTO])
                    {
                        var name = t2r.Name; //it is DisplayName, not QualifiedName
                        var metadata = t2r.Details.metadata;
                        if (!metadata.ContainsKey(AgentConstants.KEY_TESTCASE_CONTEXT))
                            _logger.Error($"Unknown context in metadata for test [{name}]");
                        var ctx = GetTestCaseContext(metadata[AgentConstants.KEY_TESTCASE_CONTEXT]);
                        if (ctx == null)
                            _logger.Error($"Tests' context in metadata is empty for test [{name}]");

                        // assembly path
                        var asmPath = ctx.AssemblyPath; //it's original tests' assembly path! Possibly, new build id located by ANOTHER path
                        if (string.IsNullOrWhiteSpace(asmPath))
                        {
                            _logger.Error($"Unknown test assembly for test [{name}]");
                            continue;
                        }

                        // we only need tests for the current TestInformer object
                        if (!AssemblyPath.Equals(asmPath, StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        //test qualified name
                        string qName = ctx.QualifiedName;
                        if (string.IsNullOrWhiteSpace(qName)) //it's possible
                            qName = TestContextHelper.GetQualifiedName(name);

                        if (delTests.Contains(qName))
                            continue;

                        //dublicates aren't needed
                        if (nameHash.Contains(qName))
                            continue;
                        nameHash.Add(qName);

                        var mustSeq = _asmOptions.DefaultParallelRestrict;
                        if (ctx.Engine != null)
                            mustSeq = ctx.Engine.MustSequential;

                        if (!seqDict.ContainsKey(asmPath))
                            seqDict.Add(asmPath, mustSeq);

                        // insert test info
                        RunAssemblyInfo asmInfo = GetOrCreateRunAssemblyInfo(dirInfo, asmPath, mustSeq);
                        asmInfo.Tests.Add(qName);
                    }
                }
                
                // new tests
                if (newTests.Count > 0) //we have the new tests that the admin side doesn't know about
                {
                    var asmInfo = GetOrCreateRunAssemblyInfo(dirInfo, AssemblyPath, seqDict.ContainsKey(AssemblyPath) ? seqDict[AssemblyPath] : _asmOptions.DefaultParallelRestrict);
                    asmInfo.Tests.AddRange(newTests);
                }
                //
                return dirInfo;
            }
            #endregion
            #region All
            if (dirInfo.RunType == RunningType.All)
            {
                var dir = _dirOptions.Directory;
                foreach (var asm in _dirOptions.Assemblies)
                {
                    var asmName = asm.DefaultAssemblyName;
                    var asmInfo = new RunAssemblyInfo
                    {
                        OrigDirectory = dir,
                        AssemblyName = asmName,
                        MustSequential = _asmOptions.DefaultParallelRestrict,
                    };
                    dirInfo.RunAssemblyInfos.Add(Path.Combine(dir, asm.DefaultAssemblyName), asmInfo);
                }
            }
            #endregion
            return dirInfo;
        }

        internal RunAssemblyInfo GetOrCreateRunAssemblyInfo(DirectoryRunInfo runInfo, string asmPath, bool mustSeq)
        {
            RunAssemblyInfo asmInfo;
            if (runInfo.RunAssemblyInfos.ContainsKey(asmPath))
            {
                asmInfo = runInfo.RunAssemblyInfos[asmPath];
            }
            else
            {
                asmInfo = new()
                {
                    OrigDirectory = Path.GetDirectoryName(asmPath),
                    AssemblyName = Path.GetFileName(asmPath),
                    MustSequential = mustSeq,
                    Tests = new(),
                };
                runInfo.RunAssemblyInfos.Add(asmPath, asmInfo);
            }
            return asmInfo;
        }

        internal Task<List<string>> GetAssemblyTests()
        {
            var asmRequester = new AssemblyRequester();
            return asmRequester.GetAssemblyTests(AssemblyPath);
        }

        internal async Task<List<TestCaseContext>> GetAssociatedTests()
        {
            var tests = await _adminRequester.GetAssociatedTests();
            List<TestCaseContext> res = new();
            foreach (var test in tests.Tests)
            {
                if (test.overview.details.metadata?.ContainsKey(AgentConstants.KEY_TESTCASE_CONTEXT) != true)
                    continue;
                var dataS = test.overview.details.metadata[AgentConstants.KEY_TESTCASE_CONTEXT];
                try
                {
                    var data = JsonConvert.DeserializeObject<TestCaseContext>(dataS);
                    var path = data.AssemblyPath;
                    if (!File.Exists(path))
                        continue;
                    if (!path.Equals(AssemblyPath, StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    res.Add(data);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Test case data was not deserialized for the test [{test.name}] in [{AssemblyPath}]", ex);
                }
            }
            return res;
        }

        internal async Task<TestToRunResponse> GetTestsToRun(bool isFake)
        {
            TestToRunResponse run = null;
            for (var i = 0; i < 5; i++) //guanito, but Drill REST service has strange behaviour
            {
                run = await (!isFake ? _adminRequester.GetTestsToRun() : GetFakeTestsToRun())
                    .ConfigureAwait(false);
                if (run.ByType != null)
                    break;
                await Task.Delay(1000).ConfigureAwait(false); //...and repeat
            }
            return run;
        }

        protected TestCaseContext GetTestCaseContext(string str)
        {
            return JsonConvert.DeserializeObject<TestCaseContext>(str);
        }

        internal async virtual Task<RunningType> GetRunningTypeByService(bool isFake)
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
                await _agentRep.RetrieveTargetBuilds().ConfigureAwait(false);
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

        //TODO: move to the normal test project
        #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        internal async virtual Task<TestToRunResponse> GetFakeTestsToRun()
        #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            TestToRunResponse tests = new();
            //TODO: create the tests in object directly, don't use deserilization here (ot use JsonNet)

            //FAKE TEST !!!
            //these tests we have to run
            //const string forRun = @"
            //{
            //    ""byType"":{
            //    ""AUTO"":[
            //        {
            //        ""name"":""PublishersArray"",
            //        ""metadata"":{
            //                 ""AssemblyPath"":""d:\\Projects\\IHS-bdd.Injected\\Ipreo.Csp.IaDeal.Api.Bdd.Tests.dll"",
            //                 ""QualifiedName"":""PublishersArray"",
            //                }
            //        },
            //        {
            //        ""name"":""BookStateUpdateFails"",
            //        ""metadata"":{
            //                ""AssemblyPath"":""d:\\Projects\\IHS-bdd.Injected\\Ipreo.Csp.IaDeal.Api.Bdd.Tests.dll"",
            //                ""QualifiedName"":""BookStateUpdateFails"",
            //               }
            //         }
            //    ]
            //    },
            //    ""totalCount"":2
            //}";
            //var opts = new System.Text.Json.JsonSerializerOptions()
            //{
            //    PropertyNameCaseInsensitive = true,
            //    AllowTrailingCommas = true,
            //};
            //return System.Text.Json.JsonSerializer.Deserialize<TestToRunResponse>(forRun, opts);

            return tests;
        }

        public override string ToString()
        {
            return $"{TargetName}: [{_dirOptions.Directory}] -> [{_asmOptions}]";
        }
    }
}
