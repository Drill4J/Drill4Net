using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
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

        public static bool IsSystemDirectory(string dir, bool rootRestricted, out string reason)
        {
            reason = null;
            if (string.IsNullOrWhiteSpace(dir))
            {
                reason = "The path is empty";
                return true;
            }

            // here we need remove ALL variants for dir separator at string ending
            if(dir.EndsWith(Path.DirectorySeparatorChar.ToString()) || dir.EndsWith("\\") || dir.EndsWith("/"))
                dir = dir.Substring(0, dir.Length - 1);

            // root disk
            if (rootRestricted && dir.Equals(Path.GetPathRoot(dir), StringComparison.InvariantCultureIgnoreCase))
            {
                reason = "The path is root volume";
                return true;
            }
            
            // user's temp folder
            if (dir.Equals(Path.GetTempPath(), StringComparison.InvariantCultureIgnoreCase))
            {
                reason = "The path is user's temp folder";
                return true;
            }
            
            // special system folders
            foreach (Environment.SpecialFolder type in Enum.GetValues(typeof(Environment.SpecialFolder)))
            {
                var sysPath = Environment.GetFolderPath(type, Environment.SpecialFolderOption.DoNotVerify); //DoNotVerify - it is important
                if (dir.Equals(sysPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    reason = $"The path is system: {type}";
                    return true;
                }
            }
            return false;
        }

        public static bool AreFoldersNested(string fullName1, string fullName2)
        {
            if (string.IsNullOrWhiteSpace(fullName1) || string.IsNullOrWhiteSpace(fullName2))
                return false;
            if (fullName1.Equals(fullName2, StringComparison.InvariantCultureIgnoreCase))
                return true;
            if (fullName1.StartsWith(fullName2, StringComparison.InvariantCultureIgnoreCase) && fullName1.Length > fullName2.Length)
                return true;
            //if (fullName2.StartsWith(fullName1, StringComparison.InvariantCultureIgnoreCase) && fullName2.Length > fullName1.Length)
                //return true;
            return false;
        }

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
