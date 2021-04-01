using System;
using System.Reflection;
#if NET5_0
using System.Runtime.Loader;
#else
using System.Collections.Generic;
#endif

namespace Drill4Net.Injector.Core
{
#if NET5_0
    //https://docs.microsoft.com/ru-ru/dotnet/core/tutorials/creating-app-with-plugin-support
    internal class AssemblyContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        /*************************************************************/

        public AssemblyContext(string asmPath) : base(isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(asmPath);
            Resolving += AssemblyContext_Resolving;
        }

        /*************************************************************/

        private Assembly AssemblyContext_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
        {
            throw new NotImplementedException();
        }

        public Assembly ResolveAssemblyToPath(string assemblyPath)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
#else
    internal class AssemblyContext
    {
        private readonly Dictionary<string, AppDomain> _domains;
        private readonly Dictionary<string, Assembly> _assemblies;

        /***************************************************************/

        public AssemblyContext()
        {
            _domains = new Dictionary<string, AppDomain>();
            _assemblies = new Dictionary<string, Assembly>();
        }

        /***************************************************************/

        public Assembly Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));
            if (_assemblies.ContainsKey(path))
            {
                return _assemblies[path];
            }
            else
            {
                var domen = AppDomain.CreateDomain(path);
                _domains.Add(path, domen);
                return domen.Load(path);
            }
        }

        public void Unload(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));
            if (!_assemblies.ContainsKey(path))
                return;
            //
            _assemblies.Remove(path);
            var domen = _domains[path];
            AppDomain.Unload(domen);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
#endif
}
