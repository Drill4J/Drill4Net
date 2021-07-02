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
            var path = FileUtils.FindAssemblyPath(shortName, version, WworkDir);
            if (path == null)
                return null;
            if (!File.Exists(path))
                throw new FileNotFoundException($"Resolve failed, file not found: [{path}]");
            var asm = Assembly.LoadFrom(path);
            _cache.Add(fullName, asm);
            return asm;
        }
    }
}
