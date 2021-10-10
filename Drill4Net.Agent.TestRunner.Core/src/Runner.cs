using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.BanderLog;

namespace Drill4Net.Agent.TestRunner.Core
{
    //Swagger: http://localhost:8090/apidocs/index.html?url=./openapi.json

    public class Runner
    {
        private readonly TestRunnerRepository _rep;
        private readonly Logger _logger;

        /***********************************************************************************/

        public Runner(TestRunnerRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _logger = new TypedLogger<Runner>(_rep.Subsystem);
        }

        /***********************************************************************************/

        public async Task Run()
        {
            _logger.Debug("Getting the build summaries...");

            try
            {
                List<BuildSummary> summary = await _rep.GetBuildSummaries().ConfigureAwait(false);
                _logger.Debug($"Builds: {summary.Count}");
                //
                var runType = RunningType.All;
                TestToRunInfo test2Run = null;
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
                    if(testCnt > 0)
                    {
                        //tests exists but no test to run (no difference between builds)
                        runType = test2runCnt == 0 ?
                            RunningType.Nothing :
                            RunningType.Certain;
                    }
                }
                _logger.Debug($"Running type: {runType}");
                //
                var tests = test2Run.ByType;
                //we need test's names here for its runs by CLI ("dotnet test ...")

                //
                _logger.Debug("Finished");
            }
            catch (Exception ex)
            {
                _logger.Fatal("Get builds' summary", ex);
            }
        }

        
    }
}
