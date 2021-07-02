using System.Collections.Generic;
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
        private static readonly Dictionary<string, AssemblyDefinition> _cache = new();

        /********************************************************************************/

        public AssemblyDefinitionResolver(string workDir):base(workDir)
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
            var path = FileUtils.FindAssemblyPath(name, nameRef.Version, WworkDir);
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
