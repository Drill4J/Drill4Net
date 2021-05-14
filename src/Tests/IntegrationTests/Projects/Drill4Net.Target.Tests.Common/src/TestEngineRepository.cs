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

namespace Drill4Net.Target.Tests.Common
{
    public class TestEngineRepository
    {
        private readonly string _targetDir;
        private readonly InjectorOptions _opts;
        private readonly FolderData _defaultFolderData;
        private readonly Dictionary<string, MonikerData> _targets;
        private readonly AssemblyContextManager _asmCtxManager;
        private readonly IInjectorRepository _injRep;

        /*******************************************************************************/

        public TestEngineRepository()
        {
            PrepareLogger();
            Log.Debug("Repository is initializing...");

            //find the cfg path
            var folderList = FileUtils.GetExecutionDir().Split('\\').ToList();

            #pragma warning disable IDE0056 // Use index operator
            var targetType = folderList[folderList.Count - 1];
            #pragma warning restore IDE0056 // Use index operator

            for (var i = 0; i < 2; i++)
                folderList.RemoveAt(folderList.Count - 1);
            var a = string.Join("\\", folderList);
            var dirName = Path.Combine(a, typeof(TestEngineRepository).Namespace, targetType);
            var cfg_path = Path.Combine(dirName, CoreConstants.CONFIG_TESTS_NAME);

            //repository
            //here better exactly InjectorRepository, not some AgentRepository
            //(for future checking the inhection settings)
            _injRep = new InjectorRepository(cfg_path); 
            _opts = _injRep.Options;
            _asmCtxManager = new AssemblyContextManager();

            var baseDir = _opts.Tests?.Directory;
            if (baseDir == null)
                Assert.Fail($"Base directory for tests is empty. See {CoreConstants.CONFIG_TESTS_NAME}");
            if (baseDir.EndsWith("\\"))
                baseDir = baseDir.Remove(baseDir.Length - 1, 1);
            _targetDir = $"{baseDir}.{_opts.Destination.FolderPostfix}";

            _defaultFolderData = new FolderData
            {
                Assemblies = new Dictionary<string, List<string>>
                {
                   { TestConstants.ASSEMBLY_COMMON,  new List<string> { TestConstants.CLASS_DEFAULT_FULL }}
                },
            };

            //target assemblies
            _targets = _opts.Tests.Targets;

            Log.Debug("Repository is initialized.");
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
                    var targetPath = Path.Combine(_targetDir, monikerData.BaseFolder, asmFile);
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

        public object LoadType(string moniker, string assemblyName, 
             string className = TestConstants.CLASS_DEFAULT_SHORT)
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
                var targerPath = Path.Combine(_targetDir, monikerData.BaseFolder, assemblyName);
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
                    var targerPath = Path.Combine(_targetDir, monikerData.BaseFolder, asm);
                    if (!File.Exists(targerPath))
                        Assert.Fail($"Path for target not found: {targerPath}. Check {CoreConstants.CONFIG_TESTS_NAME}");
                    _asmCtxManager.Unload(targerPath);
                }

            }
        }
        #endregion

        public InjectedSolution LoadTree()
        {
            //search dir with files - there must be tree data
            var curDir = _targetDir;
            while (true)
            {
                if (!Directory.Exists(curDir))
                    break;
                if (Directory.GetFiles(curDir, "*.dll").Length > 0)
                    break;
                var dirs = Directory.GetDirectories(curDir);
                if (dirs.Length == 0)
                    Assert.Fail($"Tree info not found in {_targetDir}");
                curDir = dirs[0];
            }

            //read tree data
            var treeHintPath = _injRep.GetTreeFileHintPath(curDir);
            if (!File.Exists(treeHintPath))
                Assert.Fail($"File with hint about tree data not found: {treeHintPath}");
            var treePath = File.ReadAllText(treeHintPath);
            return _injRep.ReadInjectedTree(treePath);
        }

        public void PrepareLogger()
        {
            var cfg = new LoggerHelper().GetBaseLoggerConfiguration();
            cfg.WriteTo.File(Path.Combine(FileUtils.GetCommonLogDirectory(@"..\..\..\"), "TestEngine.log"));
            Log.Logger = cfg.CreateLogger();
        }
    }
}
