#if NET5_0
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Drill4Net.Injector.Core
{
    //https://docs.microsoft.com/ru-ru/dotnet/core/tutorials/creating-app-with-plugin-support
    public class AssemblyLoaderContext : AssemblyLoadContext
    {
        private readonly AssemblyHelper _helper;
        private readonly AssemblyDependencyResolver _resolver;

        /*************************************************************/

        public AssemblyLoaderContext(string asmPath) : base(isCollectible: true)
        {
            _helper = new AssemblyHelper();
            _resolver = new AssemblyDependencyResolver(asmPath);
            Resolving += AssemblyContext_Resolving;
        }

        /*************************************************************/

        public Assembly Load(string assemblyPath)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
                return LoadFromAssemblyPath(assemblyPath);
            return null;
        }

        private Assembly AssemblyContext_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
        {
            var path = _helper.FindAssemblyPath(arg2.Name, arg2.Version);
            return path == null ? null : Assembly.LoadFrom(path);
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
                return LoadUnmanagedDllFromPath(libraryPath);

            return IntPtr.Zero;
        }
    }
}
#endif
