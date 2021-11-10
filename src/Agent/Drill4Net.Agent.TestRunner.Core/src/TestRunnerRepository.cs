﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Core.Repository;
using Drill4Net.Admin.Requester;

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
            _requester = new(Options.Url, Options.Target);
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
                var run = await (!isFake ? _requester.GetTestToRun() : GetFakeTestToRun())
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
