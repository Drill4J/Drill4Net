﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Repository;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// Repository for TestRunner
    /// </summary>
    public class TestRunnerRepository : ConfiguredRepository<TestRunnerOptions, BaseOptionsHelper<TestRunnerOptions>>
    {
        internal CliDescriptor CliDescriptor { get; }

        private List<TestAssemblyInformer> _informers;
        private readonly Logger _logger;

        /********************************************************************************/

        public TestRunnerRepository(CliDescriptor cliDescriptor): this(GetConfigPath(cliDescriptor))
        {
            CliDescriptor = cliDescriptor;
        }

        public TestRunnerRepository(string cfgPath = null): base(CoreConstants.SUBSYSTEM_TEST_RUNNER, cfgPath)
        {
            _logger = new TypedLogger<TestRunnerRepository>(Subsystem);
            if (Options.Directories?.Any() != true)
                throw new Exception("Tests' directories are empty in config");
        }

        /********************************************************************************/

        public void Init()
        {
            _informers = CreateInformers(Options.Directories, Options.Debug);
        }

        private static string GetConfigPath(CliDescriptor cliDescriptor)
        {
            var cfgPath = cliDescriptor.GetParameter(CoreConstants.ARGUMENT_CONFIG_PATH);
            if (cfgPath == null)
            {
                var aloners = cliDescriptor.GetPositionals();
                if (aloners.Count > 0)
                    cfgPath = aloners[0].Value;
            }
            return cfgPath;
        }

        internal async Task<List<DirectoryRunInfo>> GetRunInfos(CliDescriptor cliDescriptor = null)
        {
            var list = new List<DirectoryRunInfo>();
            var targetInformes = _informers.DistinctBy(a => a.TargetName); //we need to collect test2Run data just by target
            //
            var forceType = RunningType.Unknown;
            var forceTypeS = cliDescriptor?.IsSwitchSet(CoreConstants.SWITCH_FORCE_RUNNIG_TYPE_ALL);
            if (forceTypeS == true)
                forceType = RunningType.All;
            //
            foreach (var trgInformer in targetInformes.AsParallel())
            {
                DirectoryRunInfo runInfo = await trgInformer.GetRunInfo(forceType)
                    .ConfigureAwait(false);
                if (runInfo.RunType == RunningType.Nothing)
                {
                    _logger.Info($"Nothing to run for [{trgInformer}]");
                    continue;
                }
                list.Add(runInfo);
            }
            return list;
        }

        private List<TestAssemblyInformer> CreateInformers(IEnumerable<RunDirectoryOptions> dirs, TestRunnerDebugOptions dbgOpts)
        {
            // 1. No parallel execution - Connector with websocket cannot so
            // 2. No async here because inside TestInformer (and deeper) all so
            var list = new List<TestAssemblyInformer>();
            foreach (var dirOpts in dirs)
            {
                foreach(var asmOpts in dirOpts.Assemblies)
                    list.Add(new TestAssemblyInformer(dirOpts, asmOpts, dbgOpts));
            }
            //we wait here until ALL agents are initialized (and manually registered in Drill admin!)
            return list;
        }
    }
}
