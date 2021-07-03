using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Drill4Net.Common
{
    /// <summary>
    /// Common file util functions
    /// </summary>
    public static class FileUtils
    {
        /// <summary>
        /// Base directory of the running Injector App
        /// </summary>
        public static string EntryDir { get; }

        public static string ExecutingDir { get; }

        public const string LOG_FOLDER_EMERGENCY = "logs_drill";

        /******************************************************************/

        static FileUtils()
        {
            ExecutingDir = GetExecutionDir();
            EntryDir = GetEntryDir() ?? ExecutingDir;
        }

        /******************************************************************/

        public static string GetProductVersion(Type type)
        {
            return FileVersionInfo.GetVersionInfo(type.Assembly.Location).ProductVersion;
        }

        #region Directories
        public static string GetExecutionDir()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string GetCallingDir()
        {
            return Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
        }

        /// <summary>
        /// Directory for the emergency logs out of scope of the common log system
        /// </summary>
        public static string GetEntryDir()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        }

        public static string GetEmergencyDir()
        {
            return Path.Combine(GetEntryDir(), LOG_FOLDER_EMERGENCY);
        }

        public static bool IsSameDirectories(string dir1, string dir2)
        {
            if (!dir1.EndsWith("\\"))
                dir1 += "\\";
            if (!dir2.EndsWith("\\"))
                dir2 += "\\";
            return dir1.Equals(dir2, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string GetSourceDirectory(InjectorOptions opts)
        {
            return GetFullPath(opts?.Source?.Directory);
        }

        public static string GetFullPath(string path, string basePath = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;
            if (!Path.IsPathRooted(path))
                path = Path.GetFullPath(Path.Combine(basePath ?? EntryDir, path));
            return path;
        }

        public static string GetDestinationDirectory(InjectorOptions opts, string currentDir)
        {
            string destDir = GetFullPath(opts.Destination.Directory);
            if (!IsSameDirectories(currentDir, opts.Source.Directory))
            {
                var origPath = GetFullPath(opts.Source.Directory);
                destDir = Path.Combine(destDir, currentDir.Remove(0, origPath.Length));
            }
            return destDir;
        }

        public static void DirectoryCopy(string sourceDir, string destDir, bool copySubDirs = true)
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

        /// <summary>
        /// Finds the assembly path: from the frameworks's paths, work dir and the Injector dir.
        /// </summary>
        /// <param name="shortName">The short name of the assembly.</param>
        /// <param name="version">The exact version of the assembly.</param>
        /// <param name="dependenciesDir">The dependencies dir.</param>
        /// <returns></returns>
        public static string FindAssemblyPath(string shortName, Version version, string dependenciesDir = null)
        {
            //root runtime path - TODO: regex
            var curPath = RuntimeEnvironment.GetRuntimeDirectory();
            var arP = curPath.Split('\\').ToList();
            for (var i = 0; i < 3; i++)
                arP.RemoveAt(arP.Count - 1);
            var runtimeRootPath = string.Join("\\", arP);

            //runtime version
            var verS = $"{version.Major}.{version.Minor}";

            //dirs
            var dirs = Directory.GetDirectories(runtimeRootPath).ToList(); //framework's dirs
            if (!string.IsNullOrWhiteSpace(dependenciesDir)) //work dir
                dirs.Add(dependenciesDir);
            dirs.Add(EntryDir); //last chance in Injector dir...
            if (!dirs.Contains(ExecutingDir))
                dirs.Add(ExecutingDir);

            // search
            if (!shortName.EndsWith(".dll"))
                shortName = $"{shortName}.dll";
            string firstMatch = null;
            foreach (var dir in dirs)
            {
                if (!Directory.Exists(dir))
                    continue;

                //by folder as exact version
                var may = Path.Combine(dir, verS);
                var innerDirs = Directory.GetDirectories(dir);
                //first (oldest) version (hmmm....)
                foreach (var curDir in innerDirs)
                {
                    var curFPath = Path.Combine(curDir, shortName);
                    if (File.Exists(curFPath))
                    {
                        firstMatch = curFPath;
                        break;
                    }
                }
                if (firstMatch != null)
                    break;

                //folder as version by beginning
                var verDir = innerDirs.FirstOrDefault(a => a.StartsWith(may));
                var filePath = string.Empty;
                if (verDir != null)
                {
                    filePath = Path.Combine(verDir, shortName);
                    if (!File.Exists(filePath))
                        continue;
                    return filePath;
                }

                //direct in folder
                filePath = Path.Combine(dir, shortName);
                if (File.Exists(filePath))
                    return filePath;

                //may be in inner folders?! TODO: recursion into the deep
                var subDirs = Directory.GetDirectories(dir);
                foreach (var subDir in subDirs)
                {
                    filePath = Path.Combine(subDir, shortName);
                    if (File.Exists(filePath))
                        return filePath;
                }
            }

            #region from user's nuget cache
            if (firstMatch == null)
            {
                var nugetDir = $@"c:\Users\{Environment.UserName}\.nuget\packages\";
                string folder = null;
                if (shortName.EndsWith(".resources.dll"))
                    folder = shortName.Replace(".resources.dll", null);
                else
                    folder = shortName;
                nugetDir += folder;
                //
                if (Directory.Exists(nugetDir))
                {
                    var innerDirs = Directory.GetDirectories(nugetDir).OrderByDescending(a => a);
                    var may = Path.Combine(nugetDir, verS).ToLower();
                    foreach (var dir in innerDirs) //by version
                    {
                        if (!dir.ToLower().StartsWith(may))
                            continue;
                        var dirLIbs = $"{dir}\\lib";
                        //guanito with sorting. First trying get the netstandard, then netcoreapp, then net...
                        var innerDirs2 = Directory.GetDirectories(dirLIbs).OrderByDescending(a => a);
                        foreach (var dir2 in innerDirs2) //by target
                        {
                            var innerDirs3 = Directory.GetDirectories(dir2);
                            foreach (var dir3 in innerDirs3) //by language
                            {
                                var file = $"{dir3}\\{shortName}";
                                if (!File.Exists(file))
                                    continue;
                                return file;
                            }
                        }
                    }
                }
                #endregion
            }
            return firstMatch;
        }

        /// <summary>
        /// Get directory for the all subsystems of the Drill4Net
        /// </summary>
        /// <param name="relativeBaseDir"></param>
        /// <param name="logFolder"></param>
        /// <returns>Log directory's path</returns>
        public static string GetCommonLogDirectory(string relativeBaseDir, string logFolder = "logs")
        {
            return Path.Combine(GetFullPath(relativeBaseDir), logFolder);
        }
    }
}
