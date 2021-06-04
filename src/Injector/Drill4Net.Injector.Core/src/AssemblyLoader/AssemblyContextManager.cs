using System;
using System.Reflection;
using System.IO;
using System.Security.Policy;
#if NET5_0
using System.Runtime.Loader;
#endif
using System.Collections.Generic;


namespace Drill4Net.Injector.Core
{
#if NET5_0_OR_GREATER
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
#elif NETFRAMEWORK
    [Serializable]
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
                var domaininfo = new AppDomainSetup
                {
                    ApplicationBase = Path.GetDirectoryName(path)
                };
                var adevidence = AppDomain.CurrentDomain.Evidence;
                var domain = AppDomain.CreateDomain(friendlyName: path, adevidence, domaininfo);
                domain.AssemblyResolve += Domain_AssemblyResolve;
                _domains.Add(path, domain);

                byte[] rawAssembly = LoadFile(path);
                var asm = domain.Load(rawAssembly);
                _assemblies.Add(path, asm);

                domain.AssemblyResolve -= Domain_AssemblyResolve;
                return asm;
            }
        }

        private Assembly Domain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            throw new NotImplementedException();
        }

        // Loads the content of a file to a byte array.
        static byte[] LoadFile(string filename)
        {
            using FileStream fs = new FileStream(filename, FileMode.Open);
            byte[] buffer = new byte[(int)fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            return buffer;
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
#else //Core App 2.2, 3.1 
    public class AssemblyContextManager : IAssemblyContextManager
    {
        public Assembly Load(string assemblyPath)
        {
            throw new NotImplementedException();
        }

        public void Unload(string assemblyPath)
        {
            throw new NotImplementedException();
        }
    }
#endif
}
