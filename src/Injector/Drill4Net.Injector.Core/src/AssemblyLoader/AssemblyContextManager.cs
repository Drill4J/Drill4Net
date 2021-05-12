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
                refCtx = LoadAsNew(assemblyPath);
                _contexts.Add(assemblyPath, refCtx);
            }
            else
            {
                refCtx = _contexts[assemblyPath];
            }
            //
            try
            {
                var asm = ((AssemblyLoaderContext)refCtx.Target).LoadFromAssemblyPath(assemblyPath);
                return asm;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("unloaded"))
                {
                    _contexts.Remove(assemblyPath);
                    return Load(assemblyPath);
                }
                else
                    throw;
            }
        }

        private WeakReference LoadAsNew(string assemblyPath)
        {
            var asmCtx = new AssemblyLoaderContext(assemblyPath);
            return new WeakReference(asmCtx, trackResurrection: true);
        }

        public void Unload(string assemblyPath)
        {
            if (!_contexts.ContainsKey(assemblyPath))
                return;
            var refCtx = _contexts[assemblyPath];
            if (!refCtx.IsAlive)
                return;

            //https://docs.microsoft.com/en-us/dotnet/standard/assembly/unloadability
            var asmCtx = (AssemblyLoaderContext)refCtx.Target;
            try
            {
                if (refCtx.IsAlive)
                    asmCtx.Unload();
            }
            catch { }
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
