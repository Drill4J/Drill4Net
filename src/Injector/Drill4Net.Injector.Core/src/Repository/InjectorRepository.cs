using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;

namespace Drill4Net.Injector.Core
{
    public class InjectorRepository : IInjectorRepository
    {
        public MainOptions Options { get; set; }

        private readonly string _baseDir;
        private readonly string _defCfgPath;
        private readonly Deserializer _deser;

        /**********************************************************************************/

        public InjectorRepository(): 
            this(Path.Combine(GetExecutionDir(), CoreConstants.CONFIG_DEFAULT_NAME))
        {
        }

        public InjectorRepository(string cfgPath)
        {
            if (!File.Exists(cfgPath))
                throw new FileNotFoundException($"Config not found: {cfgPath}");
            _defCfgPath = cfgPath;

            //for posibility of relative pathes in misc executables: Test Engine, VS' post-build events,
            //RnD project which may located on misc levels of directories
            _baseDir = //isTestEngine ? 
                //Path.GetFullPath(Path.Combine(Path.GetDirectoryName(cfgPath), "..\\")) :
                GetExecutionDir();
            _deser = new Deserializer();
            Options = GenerateOptions();
        }

        public InjectorRepository(string[] args): this()
        {
            Options = ClarifyOptions(args);
        }

        /**********************************************************************************/

        #region Directories
        public static string GetExecutionDir()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public bool IsSameDirectories(string dir1, string dir2)
        {
            if (!dir1.EndsWith("\\"))
                dir1 += "\\";
            if (!dir2.EndsWith("\\"))
                dir2 += "\\";
            return dir1 == dir2;
        }

        public string GetSourceDirectory(MainOptions opts)
        {
            return GetFullPath(opts.Source.Directory);
        }

        public string GetFullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;
            if (!Path.IsPathRooted(path))
                path = Path.GetFullPath(Path.Combine(_baseDir, path));
            return path;
        }

        public string GetDestinationDirectory(MainOptions opts, string currentDir)
        {
            string destDir = GetFullPath(opts.Destination.Directory);
            if (!IsSameDirectories(currentDir, opts.Source.Directory))
            {
                var origPath = GetFullPath(opts.Source.Directory);
                destDir = Path.Combine(destDir, currentDir.Remove(0, origPath.Length));
            }
            return destDir;
        }

        public void CopySource(string sourcePath, string destPath)
        {
            if (Directory.Exists(destPath))
                Directory.Delete(destPath, true);
            Directory.CreateDirectory(destPath);
            DirectoryCopy(sourcePath, destPath);
        }

        public void DirectoryCopy(string sourceDir, string destDir, bool copySubDirs = true)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory does not exist: {sourceDir}");

            var dirs = dir.GetDirectories();
            Directory.CreateDirectory(destDir);

            var files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDir, file.Name);
                file.CopyTo(tempPath, false);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDir, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
        #endregion
        #region Assembly
        public IEnumerable<string> GetAssemblies(string directory)
        {
            return Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly)
                .Where(a => a.EndsWith(".exe") || a.EndsWith(".dll"));
        }

        public AssemblyVersion GetAssemblyVersion(string filePath)
        {
            var asmName = AssemblyName.GetAssemblyName(filePath);
            if (asmName.ProcessorArchitecture != ProcessorArchitecture.MSIL)
                return new AssemblyVersion() { Target = AssemblyVersionType.NotIL };
            if (!asmName.FullName.EndsWith("PublicKeyToken=null"))
            {
                //log: is strong name!
                return new AssemblyVersion() { IsStrongName = true };
            }
            var asm = Assembly.LoadFrom(filePath);
            var versionAttr = asm.CustomAttributes
                .FirstOrDefault(a => a.AttributeType == typeof(System.Runtime.Versioning.TargetFrameworkAttribute));
            var versionS = versionAttr?.ConstructorArguments[0].Value?.ToString();
            var version = new AssemblyVersion(versionS);
            return version;
        }
        #endregion
        #region Options
        internal MainOptions GenerateOptions()
        {
            return ReadOptions(_defCfgPath);
        }

        internal MainOptions ClarifyOptions(string[] args)
        {
            var cfgPath = GetCurrentConfigPath(args);
            var cfg = ReadOptions(cfgPath);
            ClarifySourceDirectory(args, cfg);
            ClarifyDestinationDirectory(args, cfg);
            return cfg;
        }

        public void NormalizePathes(MainOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            Log.Debug("Options before normalizing: {@Options}", opts);

            opts.Source.Directory = GetFullPath(opts.Source.Directory);
            opts.Destination.Directory = GetFullPath(opts.Destination.Directory);
            opts.Profiler.Directory = GetFullPath(opts.Profiler.Directory);
        }

        internal string GetCurrentConfigPath(string[] args)
        {
            var cfgArg = GetArgument(args, CoreConstants.ARGUMENT_CONFIG_PATH);
            return cfgArg == null ? _defCfgPath : cfgArg.Split('=')[1];
        }

        internal void ClarifySourceDirectory(string[] args, MainOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            //
            var sourceDir = args?.Length > 1 ? PotentialPath(args[0]) : null;
            if (!string.IsNullOrWhiteSpace(sourceDir))
                opts.Source.Directory = sourceDir;
        }

        internal void ClarifyDestinationDirectory(string[] args, MainOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            //
            var destDir = args?.Length > 1 ? PotentialPath(args?[1]) : null;
            SetDestinationDirectory(opts, destDir);
        }

        internal void SetDestinationDirectory(MainOptions opts, string destDir)
        {
            if (string.IsNullOrWhiteSpace(destDir))
                destDir = $"{Path.GetDirectoryName(opts.Source.Directory)}.{opts.Destination.FolderPostfix}";
            opts.Destination.Directory = destDir;
        }

        internal string PotentialPath(string arg)
        {
            return !arg.StartsWith("-") && (arg.Contains("//") || arg.Contains("\\")) ? arg : null;
        }

        internal string GetArgument(string[] args, string arg)
        {
            return args?.FirstOrDefault(a => a.StartsWith($"-{arg}="));
        }

        internal MainOptions ReadOptions(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Options file not found: [{path}]");
            var cfg = File.ReadAllText(path);
            var opts = _deser.Deserialize<MainOptions>(cfg);
            SetDestinationDirectory(opts, null);
            NormalizePathes(opts);
            return opts;
        }

        public void ValidateOptions(MainOptions opts)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            //
            var sourceDir = GetFullPath(opts.Source.Directory);
            if (string.IsNullOrEmpty(sourceDir))
                throw new Exception("Source directory name is empty");
            if (!Directory.Exists(sourceDir))
                throw new DirectoryNotFoundException($"Source directory does not exists: {sourceDir}");
            //
            var destDir = GetFullPath(opts.Destination.Directory);
            if (string.IsNullOrEmpty(destDir))
                throw new Exception("Destination directory name is empty");
        }

        public void ValidateOptions()
        {
            ValidateOptions(Options);
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
            return Path.Combine(tree.DestinationPath, CoreConstants.TREE_FILE_NAME);
        }

        public string GetTreeFileHintPath(string targetDir)
        {
            return Path.Combine(targetDir, CoreConstants.TREE_FILE_HINT_NAME);
        }
        #endregion
        #region Logger
        public void PrepareLogger()
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Verbose()
               .WriteTo.Console()
               .WriteTo.File(GetLogPath())
               .CreateLogger();
        }

        public string GetLogPath()
        {
            return Path.Combine(GetExecutionDir(), "logs", "log.txt");
        }
        #endregion
    }
}
