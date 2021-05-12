using Mono.Cecil;
using System.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    public class AssemblyResolver : IAssemblyResolver
    {
        private readonly AssemblyHelper _helper;
        private readonly List<AssemblyDefinition> _defs;
        private readonly ReaderParameters _readerParams;

        /*************************************************************/

        public AssemblyResolver()
        {
            _helper = new AssemblyHelper();
            _defs = new List<AssemblyDefinition>();
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

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            var path = _helper.FindAssemblyPath(name.Name, name.Version);
            var def = AssemblyDefinition.ReadAssembly(path, _readerParams);
            _defs.Add(def);
            return def;
        }

        public void Dispose()
        {
            foreach (var def in _defs)
                def.Dispose();
        }
    }
}
