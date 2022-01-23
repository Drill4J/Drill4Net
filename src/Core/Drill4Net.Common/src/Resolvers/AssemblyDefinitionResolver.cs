using System.Collections.Generic;
using System.Collections.Concurrent;
using Mono.Cecil;

namespace Drill4Net.Common
{
    /// <summary>
    /// Assembly resolver for its reading with Mono.Cecil
    /// </summary>
    /// <seealso cref="Mono.Cecil.IAssemblyResolver" />
    public class AssemblyDefinitionResolver : BaseResolver, IAssemblyResolver
    {
        private readonly ReaderParameters _readerParams;
        private static readonly ConcurrentDictionary<string, AssemblyDefinition> _cache = new();

        /********************************************************************************/

        public AssemblyDefinitionResolver(List<string> searchDirs = null) : base(searchDirs)
        {
            _readerParams = new ReaderParameters
            {
                ReadWrite = false,
                ReadingMode = ReadingMode.Immediate,
                AssemblyResolver = this,
            };
        }

        /********************************************************************************/

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
            var path = FindAssemblyPath(name, nameRef.Version);
            if (path == null)
                return null;
            try
            {
                var def = AssemblyDefinition.ReadAssembly(path, _readerParams);
                _cache.TryAdd(name, def);
                return def;
            }
            catch { return null; }
        }

        public void Dispose()
        {
            foreach (var def in _cache.Values)
                def.Dispose();
        }
    }
}
