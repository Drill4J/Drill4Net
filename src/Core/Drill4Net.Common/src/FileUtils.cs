﻿using System.IO;
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

        #region Directories
        public static string GetExecutionDir()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static bool IsSameDirectories(string dir1, string dir2)
        {
            if (!dir1.EndsWith("\\"))
                dir1 += "\\";
            if (!dir2.EndsWith("\\"))
                dir2 += "\\";
            return dir1 == dir2;
        }

        public static string GetSourceDirectory(MainOptions opts)
        {
            return GetFullPath(opts.Source.Directory);
        }

        public static string GetFullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;
            if (!Path.IsPathRooted(path))
                path = Path.GetFullPath(Path.Combine(BaseDir, path));
            return path;
        }

        public static string GetDestinationDirectory(MainOptions opts, string currentDir)
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