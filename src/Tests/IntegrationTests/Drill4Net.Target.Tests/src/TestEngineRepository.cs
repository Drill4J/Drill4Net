using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using Drill4Net.Injector.Core;

namespace Drill4Net.Target.Tests
{
    internal class TestEngineRepository
    {
        private readonly string _targetDir;
        private readonly MainOptions _opts;
        private readonly FolderData _defaultFolderData;
        private readonly Dictionary<string, MonikerData> _targets;
        private readonly AssemblyContextManager _asmCtxManager;
        private readonly IInjectorRepository _injRep;

        /*******************************************************************************/

        public TestEngineRepository()
        {
            var dirName = InjectorRepository.GetExecutionDir();
            var cfg_path = Path.Combine(dirName, CoreConstants.CONFIG_TESTS_NAME);
            _injRep = new InjectorRepository(cfg_path);
            _opts = _injRep.Options;
            _asmCtxManager = new AssemblyContextManager();

            var baseDir = _opts.Tests.Directory;
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
        }

        /*******************************************************************************/

        #region Loading target
        public void LoadTargetIntoMemory([NotNull] string moniker)
        {
            var monikerData = GetMoniker(moniker);
           
            //folders of interest in target
            foreach (var folder in monikerData.Folders)
            {
                var asms = folder.Assemblies;
                if (asms == null)
                    asms = _defaultFolderData.Assemblies;

                //asemblies
                foreach (var asm in asms.Keys)
                {
                    var targerPath = Path.Combine(_targetDir, monikerData.BaseFolder, asm);
                    if (!File.Exists(targerPath))
                        Assert.Fail($"Path for target not found: {targerPath}. Check {CoreConstants.CONFIG_TESTS_NAME}");
                    
                    //classes
                    var classes = asms[asm];
                    foreach (var classFullName in classes)
                    {
                        LoadTargetIntoMemoryByPath(targerPath, classFullName);
                    }
                }
            }
        }

        public object LoadDefaultTypeIntoMemory([NotNull] string moniker)
        {
            return LoadTypeIntoMemory(moniker, TestConstants.ASSEMBLY_COMMON);
        }

        public object LoadTypeIntoMemory([NotNull]string moniker, [NotNull] string assemblyName, 
            [NotNull] string className = TestConstants.CLASS_DEFAULT_SHORT)
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
                return LoadTargetIntoMemoryByPath(targerPath, classFullName);
            }
            return null;
        }

        internal MonikerData GetMoniker([NotNull] string moniker)
        {
            //target moniker
            if (!_targets.ContainsKey(moniker))
                throw new ArgumentException($"Moniker [{moniker}] not found in config data");
            var monikerData = _targets[moniker];
            if (monikerData == null)
                throw new ArgumentException($"Moniker data for [{moniker}] is empty");

            //folders
            if (monikerData.Folders == null)
                monikerData.Folders = new List<FolderData> { _defaultFolderData };
            return monikerData;
        }

        public object LoadTargetIntoMemoryByPath([NotNull] string assemblyPath, [NotNull] string classFullName = TestConstants.CLASS_DEFAULT_FULL)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
                throw new ArgumentNullException(nameof(assemblyPath));
            var asm = _asmCtxManager.Load(assemblyPath);
            var type = asm.GetType(classFullName);
            return Activator.CreateInstance(type);
        }

        public void UnloadTarget([NotNull] string moniker)
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
                    _asmCtxManager.Load(targerPath);
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
    }
}
