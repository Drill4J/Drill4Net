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
    public class AssemblyContext : IDisposable
    {
        /// <summary>
        /// Gets the Injector options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public InjectorOptions Options { get; }

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
        public Dictionary<string, TypeContext> TypeContexts { get; private set; }

        /// <summary>
        /// The Injected assembly's metadata
        /// </summary>
        public InjectedAssembly InjAssembly { get; set; }

        /// <summary>
        /// Injected classes of assembly's metadata
        /// </summary>
        public Dictionary<string, InjectedType> InjClasses { get; private set; }

        /// <summary>
        /// Injected methods by their full name
        /// </summary>
        public Dictionary<string, InjectedMethod> InjMethodByFullname { get; private set; }

        /// <summary>
        /// Injected methods by their special key
        /// </summary>
        public Dictionary<string, InjectedMethod> InjMethodByKeys { get; private set; }

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
        public string NameKey => $"{DestinationDir}${Definition?.FullName}${Version}";

        public string DestinationKey => Path.Combine(DestinationDir, Module?.Name); //$"{DestinationDir}{Path.DirectorySeparatorChar}{Module?.Name}";

        private bool _disposedValue;

        /***********************************************************************************/

        internal AssemblyContext(InjectorOptions options, string filePath, AssemblyVersioning version)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            SourceFile = filePath ?? throw new ArgumentNullException(nameof(filePath));
            SourceDir = $"{Path.GetFullPath(Path.GetDirectoryName(filePath) ?? string.Empty)}\\";
            DestinationDir = InjectorCoreUtils.GetDestinationDirectory(Options, SourceDir);
            Version = version;

            var ext = Path.GetExtension(filePath);
            Skipped = version == null || version.FrameworkType == AssemblyVersionType.NotIL ||
                (ext == ".exe" && version.FrameworkType == AssemblyVersionType.NetCore);

            TypeContexts = new Dictionary<string, TypeContext>();
            InjClasses = new Dictionary<string, InjectedType>();
            InjMethodByFullname = new Dictionary<string, InjectedMethod>();
            InjMethodByKeys = new Dictionary<string, InjectedMethod>();
            SubjectName = Path.GetFileNameWithoutExtension(SourceFile);
        }

        /***********************************************************************************/

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Definition?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer

                // set large fields to null
                TypeContexts = null;
                InjAssembly = null;
                InjClasses = null;
                InjMethodByFullname = null;
                InjMethodByKeys = null;

                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AssemblyContext()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);

            GC.Collect();
            GC.WaitForFullGCComplete();
            GC.WaitForPendingFinalizers();
        }
        #endregion

        public override string ToString()
        {
            return Module.Name;
        }
    }
}