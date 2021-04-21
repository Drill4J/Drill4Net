using System;
using System.Collections.Generic;
using Mono.Cecil;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class AssemblyContext
    {
        public string FilePath { get; }
        public AssemblyDefinition Definition { get; }
        
        public ModuleDefinition Module => Definition.MainModule;
        public AssemblyVersioning Version { get; }
        public InjectedAssembly InjAssembly { get; }

        public Dictionary<string, TypeContext> TypeContexts { get; }

        public Dictionary<string, InjectedType> InjClasses { get; }
        public Dictionary<string, InjectedMethod> InjMethodByFullname { get; }
        public Dictionary<string, InjectedMethod> InjMethodByKeys { get; }

        /***********************************************************************************/
        
        public AssemblyContext(string filePath, AssemblyVersioning version, AssemblyDefinition asmDef, InjectedAssembly injAssembly)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Definition = asmDef ?? throw new ArgumentNullException(nameof(asmDef));
            InjAssembly = injAssembly ?? throw new ArgumentNullException(nameof(injAssembly));
            //
            TypeContexts = new Dictionary<string, TypeContext>();
            InjClasses = new Dictionary<string, InjectedType>();
            InjMethodByFullname = new Dictionary<string, InjectedMethod>();
            InjMethodByKeys = new Dictionary<string, InjectedMethod>();
        }
        
        /***********************************************************************************/

        public override string ToString()
        {
            return Module.Name;
        }
    }
}