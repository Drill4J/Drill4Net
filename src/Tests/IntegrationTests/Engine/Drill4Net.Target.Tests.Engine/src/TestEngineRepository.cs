﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Repository;
using Drill4Net.Agent.Testing;
using Drill4Net.Configuration;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Target.Tests.Engine
{
    /// <summary>
    /// Repository for the Tester Engine subsystem
    /// </summary>
    public class TestEngineRepository
    {
        public string Subsystem { get; }

        /// <summary>
        /// Options for the Tester subsystem
        /// </summary>
        public TesterOptions Options => _tstRep?.Options;

        /// <summary>
        /// Gets the targets' root directory.
        /// </summary>
        public string TargetsDir { get; }

        /// <summary>
        /// Gets the targets from the opitons.
        /// </summary>
        /// <value>
        /// The targets.
        /// </value>
        public Dictionary<string, MonikerData> Targets { get; }

        private const string DEBUG_PROBES_FOLDER_DEFAULT = "probes";
        private readonly string _debugProbesDir;
        private readonly TestAgentRepository _tstRep;
        private readonly Logger _logger;

        /*******************************************************************************/

        /// <summary>
        /// Create the repository for the Tester subsystem
        /// </summary>
        public TestEngineRepository()
        {
            try
            {
                Subsystem = CoreConstants.SUBSYSTEM_TEST_SERVER;
                _logger = new TypedLogger<AbstractTargetTestEngine>(Subsystem);

                AbstractRepository.PrepareEmergencyLogger();
                _logger.Debug("Repository is initializing...");

                var callDir = FileUtils.CallingDir;
                var cfgDir = FindConfigInDepth(callDir);
                var cfgPath = Path.Combine(cfgDir, CoreConstants.CONFIG_NAME_TESTS);
                _tstRep = new TestAgentRepository(cfgPath);

                //targets
                Targets = Options.Versions.Targets;
                TargetsDir = _tstRep.GetTargetsDir(callDir);

                //debug - TODO: own helper based on BaseOptionsHelper for TestAgentRepository (override PostProcess method)
                var debug = Options.Debug;
                if (debug?.Disabled != true)
                {
                    _debugProbesDir = Path.Combine(callDir, DEBUG_PROBES_FOLDER_DEFAULT);
                    if (Directory.Exists(_debugProbesDir))
                        Directory.Delete(_debugProbesDir, true);
                    Directory.CreateDirectory(_debugProbesDir);
                }
                //
                _logger.Debug("Repository is initialized.");
            }
            catch (Exception ex)
            {
                _logger.Fatal($"Creating {nameof(TestEngineRepository)} is failed", ex);
            }
        }

        /*******************************************************************************/

        /// <summary>
        /// Finds the configuration by folder's hierarchy from the specified path in depth.
        /// </summary>
        /// <param name="curDir">The current directory.</param>
        /// <returns></returns>
        public string FindConfigInDepth(string curDir)
        {
            //search dir with files - there must be tree data
            while (Directory.Exists(curDir))
            {
                if (Directory.GetFiles(curDir, "*.dll").Length > 0)
                    break;
                var dirs = Directory.GetDirectories(curDir);
                if (dirs.Length == 0)
                    Assert.Fail($"Tree info not found in {TargetsDir}");
                curDir = dirs[0];
            }
            return curDir;
        }

        public Dictionary<string, MonikerData> GetTargets()
        {
            return Targets;
        }

        /// <summary>
        /// Loads the tree of the injected methods.
        /// </summary>
        /// <returns></returns>
        public InjectedSolution LoadTree()
        {
            var path = Path.Combine(TargetsDir, CoreConstants.TREE_FILE_NAME);
            return _tstRep.ReadInjectedTree(path);
        }

        /// <summary>
        /// Write to file debug info: probes data from cross-points
        /// </summary>
        /// <param name="actual"></param>
        public void WriteDebugInfo(string method, object[] args, Dictionary<string, List<PointLinkage>> actual)
        {
            if (Options.Debug.Disabled)
                return;
            List<string> strs = new();
            foreach (var func in actual.Keys)
            {
                strs.Add(func);
                var points = actual[func];
                var s = "";
                // Return type in some methods are needed
                var fltPoints = points.Where(a => a.Point.PointType != CrossPointType.Enter);
                foreach (var point in fltPoints)
                {
                    s += $"\"{point.Probe}\", ";
                }
                if (s != "")
                    s = s.Remove(s.Length - 2, 2);
                strs.Add(s);
                strs.Add(null);
            }

            //pars
            var pars = "";
            if (args?.Length > 0)
                pars += "@ " + string.Join(",", args);

            File.WriteAllLines(Path.Combine(_debugProbesDir, $"{method} {pars}.log"), strs);
        }
    }
}
