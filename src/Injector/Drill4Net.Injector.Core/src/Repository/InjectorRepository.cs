﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Serilog;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class InjectorRepository : IInjectorRepository
    {
        public MainOptions Options { get; set; }

        private readonly AssemblyContextManager _asmCtxManager = new();

        /**********************************************************************************/

        public InjectorRepository(): 
            this(Path.Combine(FileUtils.GetExecutionDir(), CoreConstants.CONFIG_DEFAULT_NAME))
        {
        }

        public InjectorRepository(string cfgPath)
        {
            if (!File.Exists(cfgPath))
                throw new FileNotFoundException($"Config not found: {cfgPath}");    
            
            OptionHelper.DefaultCfgPath = cfgPath;
            Options = OptionHelper.GenerateOptions();
        }

        public InjectorRepository(string[] args): this()
        {
            Options = OptionHelper.ClarifyOptions(args);
        }

        /**********************************************************************************/

        public virtual void CopySource(string sourcePath, string destPath, Dictionary<string, MonikerData> monikers)
        {
            if (Directory.Exists(destPath))
                Directory.Delete(destPath, true);
            Directory.CreateDirectory(destPath);

            if (monikers == null)
            {
                FileUtils.DirectoryCopy(sourcePath, destPath);
            }
            else
            {
                foreach (var moniker in monikers.Keys)
                {
                    var data = monikers[moniker];
                    var sourcePath2 = Path.Combine(sourcePath, data.BaseFolder);
                    var destPath2 = Path.Combine(destPath, data.BaseFolder);
                    FileUtils.DirectoryCopy(sourcePath2, destPath2);
                }
            }
        }

        public virtual void ValidateOptions()
        {
            OptionHelper.ValidateOptions(Options);
        }

        #region Assembly
        public virtual IEnumerable<string> GetAssemblies(string directory)
        {
            return Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly)
                .Where(a => a.EndsWith(".exe") || a.EndsWith(".dll"));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public AssemblyVersion GetAssemblyVersion(string filePath)
        {
            try
            {
                var asmName = AssemblyName.GetAssemblyName(filePath);
                if (asmName.ProcessorArchitecture != ProcessorArchitecture.MSIL)
                    return new AssemblyVersion() { Target = AssemblyVersionType.NotIL };

                if (!asmName.FullName.EndsWith("PublicKeyToken=null"))
                {
                    //log: is strong name!
                    return new AssemblyVersion() { IsStrongName = true };
                }

                var asm = _asmCtxManager.Load(filePath);
                var versionAttr = asm.CustomAttributes
                    .FirstOrDefault(a => a.AttributeType == typeof(System.Runtime.Versioning.TargetFrameworkAttribute));
                var versionS = versionAttr?.ConstructorArguments[0].Value?.ToString();
                var version = new AssemblyVersion(versionS);
                _asmCtxManager.Unload(filePath);
                return version;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Can't get the version of assembly: [{filePath}]");
                return null;
            }
        }
        #endregion
        #region Injected Tree
        public InjectedSolution ReadInjectedTree(string path)
        {
            var types = GetInjectedTreeTypes();
            var ser = new NetSerializer.Serializer(types);

            var bytes2 = File.ReadAllBytes(path);
            using var ms2 = new MemoryStream(bytes2);
            var tree = ser.Deserialize(ms2) as InjectedSolution;
            return tree;
        }

        public void WriteInjectedTree(string path, InjectedSolution tree)
        {
            var types = GetInjectedTreeTypes();
            var ser = new NetSerializer.Serializer(types);
            using var ms = new MemoryStream();
            ser.Serialize(ms, tree);
            File.WriteAllBytes(path, ms.ToArray());
        }

        internal List<Type> GetInjectedTreeTypes()
        {
            return new List<Type>
            {
                typeof(InjectedSolution),
                typeof(InjectedDirectory),
                typeof(InjectedAssembly),
                typeof(InjectedType),
                typeof(InjectedMethod),
                typeof(CrossPoint),
            };
        }

        public string GetTreeFilePath(InjectedSolution tree)
        {
            return GenerateTreeFilePath(tree.DestinationPath);
        }

        public string GenerateTreeFilePath(string targetDir)
        {
            return Path.Combine(targetDir, CoreConstants.TREE_FILE_NAME);
        }

        public string GetTreeFileHintPath(string targetDir)
        {
            return Path.Combine(targetDir, CoreConstants.TREE_FILE_HINT_NAME);
        }
        #endregion
        #region Logger
        public static void PrepareLogger()
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Verbose()
               .WriteTo.Console()
               .WriteTo.File(GetLogPath())
               .CreateLogger();
        }

        public static string GetLogPath()
        {
            return Path.Combine(FileUtils.GetExecutionDir(), "logs", "log.txt");
        }
        #endregion
    }
}
