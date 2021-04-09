using System;
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

        private InjectorRepository(): 
            this(Path.Combine(FileUtils.GetExecutionDir(), CoreConstants.CONFIG_DEFAULT_NAME))
        {
        }

        public InjectorRepository(string cfgPath)
        {
            if (!File.Exists(cfgPath))
                throw new FileNotFoundException($"Config not found: {cfgPath}");    
            Options = OptionHelper.GenerateOptions(cfgPath);
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

        public static void PrepareLogger()
        {
            var cfg = new LoggerHelper().GetBaseLoggerConfiguration();
            cfg.WriteTo.File(Path.Combine(FileUtils.GetCommonLogDirectory(@"..\..\"), $"{nameof(Injector)}.log"));
            Log.Logger = cfg.CreateLogger();
        }

        #region Assembly
        public virtual IEnumerable<string> GetAssemblies(string directory)
        {
            return Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly)
                .Where(a => a.EndsWith(".exe") || a.EndsWith(".dll"));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual AssemblyVersion GetAssemblyVersion(string filePath)
        {
            try
            {
                var asmName = AssemblyName.GetAssemblyName(filePath);
                if (asmName.ProcessorArchitecture != ProcessorArchitecture.MSIL)
                    return new AssemblyVersion() { Target = AssemblyVersionType.NotIL };

                if (!asmName.FullName.EndsWith("PublicKeyToken=null"))
                {
                    Log.Warning($"Assembly [{filePath}] having the strong name");
                    return new AssemblyVersion() { IsStrongName = true };
                }

                var asm = _asmCtxManager.Load(filePath);
                var fs = asm.DefinedTypes.FirstOrDefault(a => a.FullName?.Contains("-Target-Common-FSharp") == true);
                string versionS;
                if (fs != null)
                {
                    //var ar = fs.FullName?.Split('$'); //hm...
                    //versionS = "";
                    throw new NotImplementedException($"Processing of FSharp not implemented yet");
                }
                else
                {
                    var versionAttr = asm.CustomAttributes
                        .FirstOrDefault(a => a.AttributeType == typeof(System.Runtime.Versioning.TargetFrameworkAttribute));
                    versionS = versionAttr?.ConstructorArguments[0].Value?.ToString();
                }
                var version = new AssemblyVersion(versionS);
                _asmCtxManager.Unload(filePath);
                return version;
            }
            catch (Exception ex)
            {
                if (ex.HResult != -2146234344) //The module was expected to contain an assembly manifest
                    Log.Warning($"Getting assembly version for [{filePath}]: {ex.Message}");
                return null;
            }
        }
        #endregion
        #region Injected Tree
        public virtual InjectedSolution ReadInjectedTree(string path)
        {
            var types = GetInjectedTreeTypes();
            var ser = new NetSerializer.Serializer(types);

            var bytes2 = File.ReadAllBytes(path);
            using var ms2 = new MemoryStream(bytes2);
            var tree = ser.Deserialize(ms2) as InjectedSolution;
            return tree;
        }

        public virtual void WriteInjectedTree(string path, InjectedSolution tree)
        {
            var types = GetInjectedTreeTypes();
            var ser = new NetSerializer.Serializer(types);
            using var ms = new MemoryStream();
            ser.Serialize(ms, tree);
            File.WriteAllBytes(path, ms.ToArray());
        }

        internal virtual List<Type> GetInjectedTreeTypes()
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

        public virtual string GetTreeFilePath(InjectedSolution tree)
        {
            return GenerateTreeFilePath(tree.DestinationPath);
        }

        public virtual string GenerateTreeFilePath(string targetDir)
        {
            return Path.Combine(targetDir, CoreConstants.TREE_FILE_NAME);
        }

        public virtual string GetTreeFileHintPath(string targetDir)
        {
            return Path.Combine(targetDir, CoreConstants.TREE_FILE_HINT_NAME);
        }
        #endregion
    }
}
