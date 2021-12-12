using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// Repository for TestRunner
    /// </summary>
    public class TestRunnerRepository : ConfiguredRepository<TestRunnerOptions, BaseOptionsHelper<TestRunnerOptions>>
    {
        private List<TestInformer> _informers;
        private readonly Logger _logger;

        /********************************************************************************/

        public TestRunnerRepository(): base(CoreConstants.SUBSYSTEM_AGENT_TEST_RUNNER, string.Empty)
        {
            _logger = new TypedLogger<TestRunnerRepository>(Subsystem);
            if (Options.Directories?.Any() != true)
                throw new Exception("Tests' directories are empty in config");
        }

        /********************************************************************************/

        public void Start()
        {
            _informers = CreateInformers(Options.Directories, Options.Debug);
        }

        internal async Task<List<RunInfo>> GetRunInfos()
        {
            var list = new List<RunInfo>();
            foreach (var informer in _informers)
            {
                RunInfo runInfo = await informer.GetRunInfo().ConfigureAwait(false);
                list.Add(runInfo);
            }
            return list;
        }

        private List<TestInformer> CreateInformers(IEnumerable<RunDirectoryOptions> dirs, TestRunnerDebugOptions dbgOpts)
        {
            // 1. No parallel execution - Connector with websocket cannot so
            // 2. No async here because inside TestInformer (and deeper) all so
            var list = new List<TestInformer>();
            foreach (var dirOpts in dirs)
            {
                foreach(var asmOpts in dirOpts.Assemblies)
                    list.Add(new TestInformer(dirOpts, asmOpts, dbgOpts));
            }
            //we wait here until ALL agents are initialized (and manually registered in Dril admin!)
            return list;
        }
    }
}
