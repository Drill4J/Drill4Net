﻿using System;
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

        public static string GetCallingDir()
        {
            return Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
        }

        public static string GetEntryDir()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
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

        /// <summary>
        /// Finds the assembly path: from the frameworks's paths, work dir and the Injector dir.
        /// </summary>
        /// <param name="shortName">The short name of the assembly.</param>
        /// <param name="version">The exact version of the assembly.</param>
        /// <param name="workDir">The work dir.</param>
        /// <returns></returns>
        public static string FindAssemblyPath(string shortName, Version version, string workDir = null)
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
            if (!string.IsNullOrWhiteSpace(workDir)) //work dir
                dirs.Add(workDir);
            dirs.Add(BaseDir); //last chance in Injector dir...

            // search
            if (!shortName.EndsWith(".dll"))
                shortName = $"{shortName}.dll";
            string firstMatch = null;
            foreach (var dir in dirs)
            {
                //by folder as exact version
                var may = $"{dir}\\{verS}";
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
