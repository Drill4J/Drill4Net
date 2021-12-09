﻿using System;
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

        public static string ExecutingDir { get; }

        /***************************************************************************/

        static FileUtils()
        {
            try
            {
                ExecutingDir = GetExecutionDir() ?? @"D:\drill_debug";
                EntryDir = GetEntryDir() ?? GetCallingDir() ?? ExecutingDir;
            }
            catch(Exception ex)
            {
                File.WriteAllText(@"D:\drill_debug\FileUtils.txt", ex.ToString());
                throw;
            }
        }

        /***************************************************************************/

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

        #region Directories
        public static string GetExecutionDir()
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

        public static string GetCallingDir()
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
        public static string GetEntryDir()
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

        public static bool IsPossibleFilePath(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return false;
            return str.Contains(":") || str.Contains("..") || str.Contains("/") || str.Contains("\\");
        }

        public static bool IsSameDirectories(string dir1, string dir2)
        {
            if (!dir1.EndsWith("\\"))
                dir1 += "\\";
            if (!dir2.EndsWith("\\"))
                dir2 += "\\";
            return dir1.Equals(dir2, StringComparison.InvariantCultureIgnoreCase);
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

        public static string GetDirectoryName(string path)
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
    }
}
