using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

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

        public static string CallingDir { get; }

        public static string ExecutingDir { get; }

        /***************************************************************************/

        static FileUtils()
        {
            ExecutingDir = GetExecutingDir() ?? GetProcessDir(); //AppDomain.CurrentDomain.BaseDirectory;
            CallingDir = GetCallingDir() ?? ExecutingDir;
            EntryDir = GetEntryDir() ?? CallingDir;
        }

        /***************************************************************************/

        #region ProductVersion
        public static string GetProductVersion(Type type)
        {
            return GetProductVersion(type?.Assembly);
        }

        public static string GetProductVersion(Assembly asm)
        {
            return GetProductVersion(asm?.Location);
        }

        public static string GetProductVersion(string asmPath)
        {
            if(string.IsNullOrWhiteSpace(asmPath))
                return null;
            return FileVersionInfo.GetVersionInfo(asmPath)?.ProductVersion;
        }
        #endregion
        #region Directories
        internal static string GetExecutingDir()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                if (asm == null)
                    return null;
                return Path.GetDirectoryName(asm.Location);
            }
            catch { return null; }
        }

        internal static string GetCallingDir()
        {
            try
            {
                var asm = Assembly.GetCallingAssembly();
                if (asm == null)
                    return null;
                return Path.GetDirectoryName(asm.Location);
            }
            catch { return null; }
        }

        /// <summary>
        /// Directory for the emergency logs out of scope of the common log system
        /// </summary>
        internal static string GetEntryDir()
        {
            try
            {
                var asm = Assembly.GetEntryAssembly();
                if (asm == null)
                    return null;
                return Path.GetDirectoryName(asm.Location);
            }
            catch { return null; }
        }

        internal static string GetProcessDir()
        {
            var prc = Process.GetCurrentProcess();
            return Path.GetDirectoryName(prc.MainModule.FileName);
        }

        public static bool IsPossibleFilePath(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return false;
            return str.Contains(":") || str.Contains("..") || str.Contains('/') ||
                   str.Contains('\\') || str.Contains(Path.DirectorySeparatorChar);
        }

        public static bool IsSameDirectories(string dir1, string dir2)
        {
            dir1 = FixDirectorySeparator(dir1);
            dir2 = FixDirectorySeparator(dir2);
            return dir1.Equals(dir2, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Gets the full path from relative one.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="baseDir">The base path.</param>
        /// <returns></returns>
        public static string GetFullPath(string path, string baseDir = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;
            baseDir = FixDirectorySeparator(baseDir);
            path = FixFilePathSeparator(path);
            if (!Path.IsPathRooted(path))
                path = Path.GetFullPath(Path.Combine(baseDir ?? EntryDir, path));
            return path;
        }

        public static string FixDirectorySeparator(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;
            if (!path.EndsWith("\\") && !path.EndsWith("/"))
                path += Path.DirectorySeparatorChar;
            return FixFilePathSeparator(path);
        }

        public static string FixFilePathSeparator(string path)
        {
            return path?.Replace('/', Path.DirectorySeparatorChar)?.Replace('\\', Path.DirectorySeparatorChar);
        }

        public static string RefineDirectoryName(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            return path.EndsWith("\\") || path.EndsWith("/") ? Path.GetDirectoryName(path) : path;
        }

        public async static Task DirectoryCopy(string sourceDir, string destDir, bool copySubDirs = true)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory does not exist: {sourceDir}");

            var dirs = dir.GetDirectories();
            Directory.CreateDirectory(destDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string tempPath = Path.Combine(destDir, file.Name);
                _ = Task.Run(() => file.CopyTo(tempPath, false));
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDir, subdir.Name);
                    await DirectoryCopy(subdir.FullName, tempPath, copySubDirs)
                        .ConfigureAwait(false);
                }
            }
        }
        #endregion

        /// <summary>
        /// Does executable file exist (taking into account Linux executable)?
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool ExecutableExists(string path)
        {
            if (File.Exists(path))
                return true;
            path = Path.GetFileNameWithoutExtension(path);
            return File.Exists(path);
        }
    }
}
