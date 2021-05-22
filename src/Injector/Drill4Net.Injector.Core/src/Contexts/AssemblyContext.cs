using System;
using System.Collections.Generic;
using Mono.Cecil;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class AssemblyContext
    {
        public string SubjectName { get; set; }
        public string SourceDir { get; set; }
        public string SourceFile { get; }
        public string DestinationDir { get; set; }

        public AssemblyDefinition Definition { get; }
        public ModuleDefinition Module => Definition?.MainModule;

        public bool IsNeedPdb { get; set; }
        public bool Skipped { get; set; }

        public AssemblyVersioning Version { get; }
        public InjectedAssembly InjAssembly { get; set; }

        public Dictionary<string, TypeContext> TypeContexts { get; }

        public Dictionary<string, InjectedType> InjClasses { get; }
        public Dictionary<string, InjectedMethod> InjMethodByFullname { get; }
        public Dictionary<string, InjectedMethod> InjMethodByKeys { get; }

        public string ProxyNamespace { get; set; }
        public MethodReference ProxyMethRef { get; set; }

        public string Key => $"{Definition?.FullName}${Version}";

        /***********************************************************************************/

        internal AssemblyContext() 
        {
            TypeContexts = new Dictionary<string, TypeContext>();
            InjClasses = new Dictionary<string, InjectedType>();
            InjMethodByFullname = new Dictionary<string, InjectedMethod>();
            InjMethodByKeys = new Dictionary<string, InjectedMethod>();
        }

        public AssemblyContext(string filePath, AssemblyVersioning version, AssemblyDefinition asmDef): this()
        {
            SourceFile = filePath ?? throw new ArgumentNullException(nameof(filePath));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Definition = asmDef ?? throw new ArgumentNullException(nameof(asmDef));
        }
        
        /***********************************************************************************/

        public override string ToString()
        {
            return Module.Name;
        }
    }
}