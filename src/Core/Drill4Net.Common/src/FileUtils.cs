using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Drill4Net.Common
{
    public static class FileUtils
    {
        public static string BaseDir { get; }

        /******************************************************************/

        static FileUtils()
        {
            BaseDir = GetExecutionDir();
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

        public static string GetEntryDir()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
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
            return GetFullPath(opts.Source.Directory);
        }

        public static string GetFullPath(string path, string basePath = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;
            if (!Path.IsPathRooted(path))
                path = Path.GetFullPath(Path.Combine(basePath ?? BaseDir, path));
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

        public static string GetCommonLogDirectory(string relativeBaseDir, string logFoler = "logs")
        {
            return Path.Combine(GetFullPath(relativeBaseDir), logFoler);
        }
    }
}
