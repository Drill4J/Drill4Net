using System;
using System.Collections.Generic;
using Drill4Net.Profiling.Tree;
using Mono.Cecil;

namespace Drill4Net.Injector.Core
{
    public class AssemblyContext
    {
        public string FilePath { get; }
        public AssemblyDefinition Definition { get; }
        
        public ModuleDefinition Module => Definition.MainModule;
        public AssemblyVersion Version { get; }
        public InjectedAssembly InjAssembly { get; }

        public List<TypeContext> TypeContexts { get; }

        public Dictionary<string, InjectedType> InjClasses { get; }
        public Dictionary<string, InjectedMethod> InjMethods { get; }
        public Dictionary<string, InjectedMethod> InjMethodByClasses { get; }

        /********************************************************************************/
        
        public AssemblyContext(string filePath, AssemblyVersion version, AssemblyDefinition asmDef, InjectedAssembly injAssembly)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Definition = asmDef ?? throw new ArgumentNullException(nameof(asmDef));
            InjAssembly = injAssembly ?? throw new ArgumentNullException(nameof(injAssembly));
            //
            TypeContexts = new List<TypeContext>();
            InjClasses = new Dictionary<string, InjectedType>();
            InjMethods = new Dictionary<string, InjectedMethod>();
            InjMethodByClasses = new Dictionary<string, InjectedMethod>();
        }
    }
}