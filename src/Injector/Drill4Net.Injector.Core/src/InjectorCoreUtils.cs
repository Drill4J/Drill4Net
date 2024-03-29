﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.Configuration;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Some core utils for Injector
    /// </summary>
    public static class InjectorCoreUtils
    {
        public static string GetDestinationDirectory(InjectionOptions opts, string currentDir)
        {
            string destDir = FileUtils.GetFullPath(opts.Destination.Directory);
            if (currentDir.StartsWith(destDir)) // if we inject already injected assembly (plugins)
                return currentDir;
            if (!FileUtils.IsSameDirectories(currentDir, opts.Source.Directory))
            {
                var origPath = FileUtils.GetFullPath(opts.Source.Directory);
                destDir = Path.Combine(destDir, currentDir.Remove(0, origPath.Length));
            }
            return destDir;
        }

        public static bool IsDirectoryNeedByMoniker(Dictionary<string, MonikerData> monikers, string root, string dir)
        {
            //filter by target moniker (typed version)
            var need = monikers == null || monikers.Count == 0 || monikers.Any(a =>
            {
                var x = Path.Combine(root, a.Value.BaseFolder);
                if (x.EndsWith("\\"))
                    x = x[0..^1];
                if (x.Equals(dir, StringComparison.InvariantCultureIgnoreCase))
                    return true;
                var z = Path.Combine(dir, a.Key);
                return x.Equals(z, StringComparison.InvariantCultureIgnoreCase);
            });
            return need;
        }

        public static bool IsNeedProcessDirectory(SourceFilterOptions flt, string directory, bool isRoot)
        {
            if (isRoot || flt == null)
                return true;
            if (!flt.IsDirectoryNeed(directory))
                return false;
            var folder = new DirectoryInfo(directory).Name;
            return flt.IsFolderNeed(folder);
        }

        public static bool IsNeedProcessFile(SourceFilterOptions flt, string filePath)
        {
            return flt?.IsFileNeedByPath(Path.GetFileName(filePath)) != false;
        }
    }
}
