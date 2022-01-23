using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Drill4Net.Common
{
    public abstract class BaseResolver
    {
        public List<string> SearchDirs { get; }

        private readonly List<string> _runtimeDirs;

        /************************************************************************/

        protected BaseResolver(List<string> searchDirs = null)
        {
            SearchDirs = searchDirs ?? new List<string>();
            if(!SearchDirs.Any())
                SearchDirs.Add(FileUtils.EntryDir);
            var runtimeRootPath = GetRuntimeDir();
            _runtimeDirs = Directory.GetDirectories(runtimeRootPath).ToList();
        }

        /************************************************************************/

        /// <summary>
        /// Finds the assembly path: from the frameworks's paths, work dir and the Injector dir.
        /// </summary>
        /// <param name="shortName">The short name of the assembly.</param>
        /// <param name="version">The exact version of the assembly.</param>
        /// <param name="dependenciesDir">The dependencies dir.</param>
        /// <returns></returns>
        public string FindAssemblyPath(string shortName, Version version, string dependenciesDir = null)
        {
            //dirs
            var dirs = new List<string>();
            dirs.AddRange(SearchDirs);
            if (!string.IsNullOrWhiteSpace(dependenciesDir) && !dirs.Contains(dependenciesDir))
                dirs.Add(dependenciesDir);
            if (!dirs.Contains(FileUtils.EntryDir))
                dirs.Add(FileUtils.EntryDir);
            if (!dirs.Contains(FileUtils.ExecutingDir))
                dirs.Add(FileUtils.ExecutingDir);
            foreach (var dir in _runtimeDirs)
            {
                if (!dirs.Contains(dir))
                    dirs.Add(dir);
            }

            var verS = $"{version.Major}.{version.Minor}"; //runtime version
            if (!shortName.EndsWith(".dll"))
                shortName = $"{shortName}.dll";
            string firstMatch = null;

            // search 
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

                //direct in folder - TODO: check the Product version???
                filePath = Path.Combine(dir, shortName);
                if (File.Exists(filePath))
                    return filePath;

                //maybe in inner folders?! TODO: recursion into the deep
                var subDirs = Directory.GetDirectories(dir);
                foreach (var subDir in subDirs)
                {
                    filePath = Path.Combine(subDir, shortName);
                    if (File.Exists(filePath))
                        return filePath;
                }
            }

            #region User's nuget cache
            if (firstMatch == null)
            {
                var nugetDir = Path.Combine(CommonUtils.GetUserDir(), ".nuget", "packages") + Path.DirectorySeparatorChar;
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
                        var dirLIbs = $"{dir}{Path.DirectorySeparatorChar}lib";
                        //guanito with sorting. First trying get the netstandard, then netcoreapp, then net...
                        var innerDirs2 = Directory.GetDirectories(dirLIbs).OrderByDescending(a => a);
                        foreach (var dir2 in innerDirs2) //by target
                        {
                            var innerDirs3 = Directory.GetDirectories(dir2);
                            foreach (var dir3 in innerDirs3) //by language
                            {
                                var file = $"{dir3}{Path.DirectorySeparatorChar}{shortName}";
                                if (!File.Exists(file))
                                    continue;
                                return file;
                            }
                        }
                    }
                }
            }
            #endregion
            return firstMatch;
        }

        internal string GetRuntimeDir()
        {
            //root runtime path - TODO: regex
            var curPath = RuntimeEnvironment.GetRuntimeDirectory();
            var arP = curPath.Split(Path.DirectorySeparatorChar).ToList();
            for (var i = 0; i < arP.Count - 2; i++)
                arP.RemoveAt(arP.Count - 1);
            return string.Join(Path.DirectorySeparatorChar.ToString(), arP);
        }
    }
}
