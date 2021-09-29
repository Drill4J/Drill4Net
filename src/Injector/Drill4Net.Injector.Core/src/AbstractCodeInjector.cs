using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;

namespace Drill4Net.Injector.Core
{
    public abstract class AbstractCodeInjector : IDisposable
    {
        protected readonly Dictionary<bool, ModuleDefinition> _syslibs;

        /*****************************************************************************************/

        protected AbstractCodeInjector(Dictionary<bool, ModuleDefinition> syslibs = null)
        {
            _syslibs = syslibs ?? new Dictionary<bool, ModuleDefinition>();
        }

        /*****************************************************************************************/

        public abstract void InjectTo(AssemblyDefinition assembly, string proxyNs, bool isNetFX = false);

        protected MethodReference ImportSysMethodReference(ModuleDefinition syslib, ModuleDefinition target,
            string ns, string typeName, string method, bool isStatic,
            Type parType, Type resType)
        {
            return ImportSysMethodReference(syslib, target, ns, typeName, method, isStatic, new Type[] { parType }, resType);
        }

        protected MethodReference ImportSysMethodReference(ModuleDefinition syslib, ModuleDefinition target,
            string ns, string typeName, string method, bool isStatic,
            Type[] parTypes, Type resType)
        {
            var methTypeRef = new TypeReference(ns, typeName, syslib, syslib);
            var resTypeRef = new TypeReference(resType.Namespace, resType.Name, syslib, syslib);
            var methRef = new MethodReference(method, resTypeRef, methTypeRef);
            if (!isStatic)
                methRef.HasThis = true;

            //parameters
            for (int i = 0; i < parTypes.Length; i++)
            {
                var parDef = CreateParameterDefinition($"par_{i}", parTypes[i], syslib);
                methRef.Parameters.Add(parDef);
            }

            return target.ImportReference(methRef);
        }

        protected ParameterDefinition CreateParameterDefinition(string parName, Type parType, ModuleDefinition module)
        {
            var parTypeRef = new TypeReference(parType.Namespace, parType.Name, module, module);
            return new ParameterDefinition(parName, ParameterAttributes.None, parTypeRef);
        }

        protected ParameterDefinition ImportParameterDefinition(string parName, Type parType, ModuleDefinition from, ModuleDefinition target)
        {
            var parTypeRef = target.ImportReference(new TypeReference(parType.Namespace, parType.Name, from, from));
            return new ParameterDefinition(parName, ParameterAttributes.None, parTypeRef);
        }

        protected TypeReference ImportSysTypeReference(ModuleDefinition syslib, ModuleDefinition target, Type type)
        {
            return target.ImportReference(new TypeReference(type.Namespace, type.Name, syslib, syslib));
        }

        protected ModuleDefinition GetSysModule(bool isNetFx)
        {
            if (_syslibs.ContainsKey(isNetFx))
                return _syslibs[isNetFx];
            var module = ModuleDefinition.ReadModule(GetSysLibPath(isNetFx), new ReaderParameters());
            _syslibs.Add(isNetFx, module);
            return module;
        }

        protected string GetSysLibPath(bool isNetFx)
        {
            //TODO: get real root dirs from Environment
            var root = isNetFx ?
                @"c:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\" :
                @"c:\Program Files (x86)\dotnet\shared\Microsoft.NETCore.App\";
            var pattern = isNetFx ? "v4.*" : "5.*"; //TODO: for next versions
            var fileName = isNetFx ? "mscorlib.dll" : "System.Private.CoreLib.dll";
            var dirs = Directory.GetDirectories(root, pattern, SearchOption.TopDirectoryOnly)
                .Where(a => !a.Contains("X")) //NetFx folder without libs
                .OrderBy(a => a)
                .ToArray();
            if (dirs.Length == 0)
                throw new Exception("System lib's directory not found");
            var path = Path.Combine(dirs[dirs.Length - 1], fileName);
            if (!File.Exists(path))
                throw new Exception("System lib not found");
            return path;
        }

        //TODO: full pattern!
        public virtual void Dispose()
        {
            foreach (var lib in _syslibs.Values)
                lib.Dispose();
        }
    }
}
