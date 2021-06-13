using Mono.Cecil;
using System.Collections.Generic;

namespace Drill4Net.Common
{
    public class AssemblyResolver : IAssemblyResolver
    {
        private readonly ReaderParameters _readerParams;
        private readonly Dictionary<string, AssemblyDefinition> _cache;

        /*************************************************************/

        public AssemblyResolver()
        {
            _cache = new Dictionary<string, AssemblyDefinition>();
            _readerParams = new ReaderParameters
            {
                ReadWrite = false,
                ReadingMode = ReadingMode.Immediate,
            };
        }

        /*************************************************************/

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return Resolve(name, null);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference nameRef, ReaderParameters parameters)
        {
            var name = nameRef.Name;
            if (_cache.ContainsKey(name))
                return _cache[name];
            //
            var path = FileUtils.FindAssemblyPath(name, nameRef.Version);
            if (path == null)
                return null;
            var def = AssemblyDefinition.ReadAssembly(path, _readerParams);
            _cache.Add(name, def);
            return def;
        }

        public void Dispose()
        {
            foreach (var def in _cache.Values)
                def.Dispose();
        }
    }
}
