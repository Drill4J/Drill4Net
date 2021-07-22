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
            return GetProductVersion(type.Assembly.Location);
        }

        public static string GetProductVersion(string asmPath)
        {
            return FileVersionInfo.GetVersionInfo(asmPath).ProductVersion;
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

        /// <summary>
        /// Gets the full path from relative one.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="basePath">The base path.</param>
        /// <returns></returns>
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
