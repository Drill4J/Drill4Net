using System;
using System.IO;
using System.Collections.Generic;
using Mono.Cecil;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Context of the assembly 's current data
    /// </summary>
    public class AssemblyContext
    {
        /// <summary>
        /// Subject name - just short name of the assembly
        /// </summary>
        public string SubjectName { get; set; }

        /// <summary>
        /// Source directory of the target assembly
        /// </summary>
        public string SourceDir { get; set; }

        /// <summary>
        /// File name of the target assembly
        /// </summary>
        public string SourceFile { get; }

        /// <summary>
        /// Destination directory of the injected assembly
        /// </summary>
        public string DestinationDir { get; set; }

        /// <summary>
        /// The definition of the assembly
        /// </summary>
        public AssemblyDefinition Definition { get; set; }

        /// <summary>
        /// Main module of the assembly
        /// </summary>
        public ModuleDefinition Module => Definition?.MainModule;

        /// <summary>
        /// Do need use the PDB info?
        /// </summary>
        public bool IsNeedPdb { get; set; }

        /// <summary>
        /// Do the assembly need to be injected?
        /// </summary>
        public bool Skipped { get; set; }

        /// <summary>
        /// Version info of assembly
        /// </summary>
        public AssemblyVersioning Version { get; }

        /// <summary>
        /// Type contexts of assembly
        /// </summary>
        public Dictionary<string, TypeContext> TypeContexts { get; }

        /// <summary>
        /// The Injected assembly's metadata
        /// </summary>
        public InjectedAssembly InjAssembly { get; set; }

        /// <summary>
        /// Injected classes of assembly's metadata
        /// </summary>
        public Dictionary<string, InjectedType> InjClasses { get; }

        /// <summary>
        /// Injected methods by their full name
        /// </summary>
        public Dictionary<string, InjectedMethod> InjMethodByFullname { get; }

        /// <summary>
        /// Injected methods by their special key
        /// </summary>
        public Dictionary<string, InjectedMethod> InjMethodByKeys { get; }

        /// <summary>
        /// The Proxy's namespace
        /// </summary>
        public string ProxyNamespace { get; set; }

        /// <summary>
        /// The Proxy's method reference
        /// </summary>
        public MethodReference ProxyMethRef { get; set; }

        /// <summary>
        /// The Key of the assembly
        /// </summary>
        public string Key => $"{Definition?.FullName}${Version}";

        /***********************************************************************************/

        internal AssemblyContext(string filePath, AssemblyVersioning version)
        {
            SourceFile = filePath ?? throw new ArgumentNullException(nameof(filePath));
            Version = version;
            TypeContexts = new Dictionary<string, TypeContext>();
            InjClasses = new Dictionary<string, InjectedType>();
            InjMethodByFullname = new Dictionary<string, InjectedMethod>();
            InjMethodByKeys = new Dictionary<string, InjectedMethod>();
            SubjectName = Path.GetFileNameWithoutExtension(SourceFile);
        }

        public AssemblyContext(string filePath, AssemblyVersioning version, AssemblyDefinition asmDef): this(filePath, version)
        {
            if (Version == null)
                throw new InvalidDataException(nameof(version));
            Definition = asmDef ?? throw new ArgumentNullException(nameof(asmDef));
        }
        
        /***********************************************************************************/

        public override string ToString()
        {
            return Module.Name;
        }
    }
}