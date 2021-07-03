using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Drill4Net.Common
{
    public class AssemblyResolver : BaseResolver
    {
        private static readonly Dictionary<string, Assembly> _cache = new();

        /********************************************************************************/

        public AssemblyResolver(string wworkDir = null) : base(wworkDir)
        {
        }

        /********************************************************************************/

        public Assembly Resolve(string fullName)
        {
            if (_cache.ContainsKey(fullName))
                return _cache[fullName];
            var (shortName, version) = CommonUtils.ParseAssemblyVersion(fullName);
            var path = FileUtils.FindAssemblyPath(shortName, version, WorkDir);
            if (path == null)
                return null;
            if (!File.Exists(path))
                throw new FileNotFoundException($"Resolve failed, file not found: [{path}]");
            var asm = Assembly.LoadFrom(path);
            _cache.Add(fullName, asm);
            return asm;
        }

        public Assembly ResolveResource(string requestingAssemblyPath, string resource)
        {
            if (!File.Exists(requestingAssemblyPath))
                return null;
            if (string.IsNullOrWhiteSpace(resource) || !resource.Contains("."))
                return null;
            var ar = resource.Split('.');
            var localization = ar[ar.Length - 2];
            var dir = Path.GetDirectoryName(requestingAssemblyPath);
            var dir2 = Path.Combine(new DirectoryInfo(dir).Parent.FullName, localization);
            var path = Path.Combine(dir2, Path.GetFileName(requestingAssemblyPath));
            if (!File.Exists(path))
                path = requestingAssemblyPath;
            return Assembly.LoadFrom(path);
        }
    }
}
