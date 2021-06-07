using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Serilog;
using NUnit.Framework;
using Drill4Net.Injector.Core;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;
using Drill4Net.Agent.Testing;

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
        private readonly FolderData _defaultFolderData;
        private readonly Dictionary<string, MonikerData> _targets;
        private readonly AssemblyContextManager _asmCtxManager;
        private readonly TesterRepository _tstRep;

        /*******************************************************************************/

        /// <summary>
        /// Create the repository for the Tester subsystem
        /// </summary>
        public TesterEngineRepository()
        {
            try
            {
                PrepareLogger();
                Log.Debug("Repository is initializing...");

                var callDir = FileUtils.GetCallingDir();
                var cfgDir = FindConfigDeep(callDir);
                var cfg_path = Path.Combine(cfgDir, CoreConstants.CONFIG_TESTS_NAME);
                _tstRep = new TesterRepository(cfg_path);
                _targetsDir = _tstRep.GetTargetsDir(callDir);

                _asmCtxManager = new AssemblyContextManager();
                _defaultFolderData = new FolderData
                {
                    Assemblies = new Dictionary<string, List<string>>
                    {
                       { TestConstants.ASSEMBLY_COMMON,  new List<string> { TestConstants.CLASS_DEFAULT_FULL }}
                    },
                };

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

        #region Loading target
        public Dictionary<string, object> LoadTarget(string moniker)
        {
            var monikerData = GetMoniker(moniker);

            //folders of interest in target
            var objects = new Dictionary<string, object>();
            foreach (var folder in monikerData.Folders)
            {
                var asmDatas = folder.Assemblies;
                if (asmDatas == null)
                    asmDatas = _defaultFolderData.Assemblies;

                //asemblies
                foreach (var asmFile in asmDatas.Keys)
                {
                    var targetPath = Path.Combine(_targetsDir, monikerData.BaseFolder, asmFile);
                    if (!File.Exists(targetPath))
                        Assert.Fail($"Path for target not found: {targetPath}. Check {CoreConstants.CONFIG_TESTS_NAME}");
                    
                    //classes
                    var classes = asmDatas[asmFile];
                    foreach (var classFullName in classes)
                    {
                        var asm = LoadAssembly(targetPath);
                        var obj = LoadType(asm, classFullName);
                        objects.Add(classFullName, obj);
                    }
                }
            }
            return objects;
        }

        public object LoadDefaultType(string moniker)
        {
            return LoadType(moniker, TestConstants.ASSEMBLY_COMMON);
        }

        public object LoadType(string moniker, string assemblyName, string className = TestConstants.CLASS_DEFAULT_SHORT)
        {
            var monikerData = GetMoniker(moniker);

            //folders of interest in target
            foreach (var folder in monikerData.Folders)
            {
                //assemblies
                var asms = folder.Assemblies;
                if (asms == null)
                    asms = _defaultFolderData.Assemblies;
                if (!asms.ContainsKey(assemblyName))
                    throw new ArgumentException($"Assembly [{assemblyName}] not found in moniker [{moniker}]");

                //class
                List<string> classes = asms[assemblyName];
                var classFullName = classes.FirstOrDefault(a => a.EndsWith($".{className}", StringComparison.InvariantCultureIgnoreCase));
                if (classFullName == null)
                    throw new ArgumentException($"Class [{className}] not found for assembly [{assemblyName}] in config");

                //loading
                var targerPath = Path.Combine(_targetsDir, monikerData.BaseFolder, assemblyName);
                if (!File.Exists(targerPath))
                    Assert.Fail($"Path for target not found: {targerPath}. Check {CoreConstants.CONFIG_TESTS_NAME}");
                var asm = LoadAssembly(targerPath);
                return LoadType(asm, classFullName);
            }
            return null;
        }

        internal MonikerData GetMoniker(string moniker)
        {
            //target moniker
            if (!_targets.ContainsKey(moniker))
                Assert.Ignore($"Moniker [{moniker}] not found in config data");
            var monikerData = _targets[moniker];
            if (monikerData == null)
                throw new ArgumentException($"Moniker data for [{moniker}] is empty");

            //folders
            if (monikerData.Folders == null)
                monikerData.Folders = new List<FolderData> { _defaultFolderData };
            return monikerData;
        }

        public Assembly LoadAssembly(string assemblyPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
                throw new ArgumentNullException(nameof(assemblyPath));
            return _asmCtxManager.Load(assemblyPath);
        }

        internal object LoadType( Assembly asm,  string classFullName = TestConstants.CLASS_DEFAULT_FULL)
        {
            var type = asm.GetType(classFullName);
            return Activator.CreateInstance(type);
        }

        public void UnloadTarget(string moniker)
        {
            var monikerData = GetMoniker(moniker);
            foreach (var folder in monikerData.Folders)
            {
                var asms = folder.Assemblies;
                if (asms == null)
                    asms = _defaultFolderData.Assemblies;
                foreach (var asm in asms.Keys)
                {
                    var targerPath = Path.Combine(_targetsDir, monikerData.BaseFolder, asm);
                    if (!File.Exists(targerPath))
                        Assert.Fail($"Path for target not found: {targerPath}. Check {CoreConstants.CONFIG_TESTS_NAME}");
                    _asmCtxManager.Unload(targerPath);
                }

            }
        }
        #endregion

        public string FindConfigDeep(string curDir)
        {
            //search dir with files - there must be tree data
            while (true)
            {
                if (!Directory.Exists(curDir))
                    break;
                if (Directory.GetFiles(curDir, "*.dll").Length > 0)
                    break;
                var dirs = Directory.GetDirectories(curDir);
                if (dirs.Length == 0)
                    Assert.Fail($"Tree info not found in {_targetsDir}");
                curDir = dirs[0];
            }
            return curDir;
        }

        public InjectedSolution LoadTree()
        {
            var path = Path.Combine(_targetsDir, CoreConstants.TREE_FILE_NAME);
            return _tstRep.ReadInjectedTree(path);
        }

        public void PrepareLogger()
        {
            var cfg = new LoggerHelper().GetBaseLoggerConfiguration();
            cfg.WriteTo.File(Path.Combine(FileUtils.GetCommonLogDirectory(@"..\..\..\..\..\"), "TestEngine.log"));
            Log.Logger = cfg.CreateLogger();
        }
    }
}
