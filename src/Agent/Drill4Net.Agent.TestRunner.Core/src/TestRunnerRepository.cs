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

        private List<TestInformer> CreateInformers(IEnumerable<RunDirectoryOptions> dirs, TestRunnerDebugOptions dbgOpts)
        {
            //https://stackoverflow.com/questions/30225476/task-run-with-parameters
            var list = new List<TestInformer>();
            // 1. No parallel execution - Connector cannot so
            // 2. No async here because inside TestInformer and deeper all so
            foreach (var dirOpts in dirs)
            {
                foreach(var asmOpts in dirOpts.Assemblies)
                    list.Add(new TestInformer(dirOpts, asmOpts, dbgOpts));
            }
            //we wait here until all agents are initialized (and manually registered in Dril admin!)
            return list;
        }
    }
}
