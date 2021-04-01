using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Drill4Net.Injector.Core
{
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
}
