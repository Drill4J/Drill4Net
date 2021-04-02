using System;
using System.Reflection;
#if NET5_0
using System.Runtime.Loader;
#endif
using System.Collections.Generic;


namespace Drill4Net.Injector.Core
{
#if NET5_0
    public class AssemblyContextManager : IAssemblyContextManager
    {
        private readonly Dictionary<string, WeakReference> _contexts;

        /*************************************************************/

        public AssemblyContextManager()
        {
            _contexts = new Dictionary<string, WeakReference>();
        }

        /*************************************************************/

        public Assembly Load(string assemblyPath)
        {
            WeakReference refCtx;
            if (!_contexts.ContainsKey(assemblyPath))
            {
                var asmCtx = new AssemblyContext(assemblyPath);
                refCtx = new WeakReference(asmCtx, trackResurrection: true);
                _contexts.Add(assemblyPath, refCtx);
            }
            else
            {
                refCtx = _contexts[assemblyPath];
            }
            var asm = ((AssemblyContext)refCtx.Target).LoadFromAssemblyPath(assemblyPath);
            return asm;
        }

        public void Unload(string assemblyPath)
        {
            if (!_contexts.ContainsKey(assemblyPath))
                return;
            var refCtx = _contexts[assemblyPath];
            if (!refCtx.IsAlive)
                return;

            //https://docs.microsoft.com/en-us/dotnet/standard/assembly/unloadability
            var asmCtx = (AssemblyContext)refCtx.Target;
            asmCtx.Unload();
            for (var i = 0; refCtx.IsAlive && i < 10; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
#else
    public class AssemblyContextManager : IAssemblyContextManager
    {
        private readonly Dictionary<string, AppDomain> _domains;
        private readonly Dictionary<string, Assembly> _assemblies;

        /***************************************************************/

        public AssemblyContextManager()
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
            var domain = _domains[path];

            //TODO: it's simplified... do it properly
            AppDomain.Unload(domain);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
#endif
}
