using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.BanderLog;

namespace Drill4Net.Agent.TestRunner.Core
{
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
            _logger.Debug("Running...");

            try
            {
                List<BuildSummary> summary = await _rep.GetBuildSummaries();
                _logger.Debug($"Builds: {summary.Count}");
                //
                var runType = RunningType.All;
                TestToRunInfo test2Run = null;
                if (summary.Count > 0)
                {
                    summary = summary.OrderByDescending(a => a.DetectedAt).ToList();
                    var last = summary[0];
                    test2Run = last?.Summary?.TestsToRun;
                    if (test2Run == null)
                        throw new Exception("No info about test2Run");
                    _logger.Debug($"Test to run: {test2Run.Count}");

                    runType = test2Run.Count == 0 ? RunningType.Nothing : RunningType.Certain;
                }
                _logger.Debug($"Running type: {runType}");
                //
                var tests = test2Run.ByType;

                //
                _logger.Debug("Finished");
            }
            catch (Exception ex)
            {
                _logger.Fatal("Get summary", ex);
            }
        }

        
    }
}
