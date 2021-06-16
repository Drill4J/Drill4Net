using System;
using System.IO;
using System.Collections.Generic;
using Serilog;
using NUnit.Framework;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;
using Drill4Net.Agent.Testing;
using Drill4Net.Core.Repository;

namespace Drill4Net.Target.Tests.Common
{
    /// <summary>
    /// Repository for the Tester subsystem
    /// </summary>
    public class TesterEngineRepository
    {
        /// <summary>
        /// Options for the Tester subsystem
        /// </summary>
        public TesterOptions Options => _tstRep?.Options;

        private readonly string _targetsDir;
        private readonly Dictionary<string, MonikerData> _targets;
        private readonly TesterRepository _tstRep;

        /*******************************************************************************/

        /// <summary>
        /// Create the repository for the Tester subsystem
        /// </summary>
        public TesterEngineRepository()
        {
            try
            {
                BaseRepository.PrepareInitLogger();
                Log.Debug("Repository is initializing...");

                var callDir = FileUtils.GetCallingDir();
                var cfgDir = FindConfigInDepth(callDir);
                var cfg_path = Path.Combine(cfgDir, CoreConstants.CONFIG_TESTS_NAME);
                _tstRep = new TesterRepository(cfg_path);
                _targetsDir = _tstRep.GetTargetsDir(callDir);

                //target assemblies
                _targets = Options.Versions.Targets;

                Log.Debug("Repository is initialized.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"Creating {nameof(TesterEngineRepository)} is failed");
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
                    Assert.Fail($"Tree info not found in {_targetsDir}");
                curDir = dirs[0];
            }
            return curDir;
        }

        /// <summary>
        /// Loads the tree of the injected methods.
        /// </summary>
        /// <returns></returns>
        public InjectedSolution LoadTree()
        {
            var path = Path.Combine(_targetsDir, CoreConstants.TREE_FILE_NAME);
            return _tstRep.ReadInjectedTree(path);
        }

        //public void PrepareLogger()
        //{
        //    var cfg = new LoggerHelper().GetBaseLoggerConfiguration();
        //    cfg.WriteTo.File(Path.Combine(FileUtils.GetCommonLogDirectory(@"..\..\..\..\..\"), $"{nameof(TesterEngineRepository)}.log"));
        //    Log.Logger = cfg.CreateLogger();
        //}
    }
}
