using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Drill4Net.Common;
using Drill4Net.Configuration;

namespace Drill4Net.Injector.Core
{
    public abstract class AbstractCodeInjector : IDisposable
    {
        public const string METHOD_COMMAND_NAME = "DoCommand";
        protected readonly Dictionary<bool, ModuleDefinition> _syslibs;

        /*****************************************************************************************/

        protected AbstractCodeInjector(Dictionary<bool, ModuleDefinition> syslibs = null)
        {
            _syslibs = syslibs ?? new Dictionary<bool, ModuleDefinition>();
        }

        /*****************************************************************************************/

        public abstract void InjectTo(AssemblyDefinition assembly, string proxyNs, bool isNetFX = false);

        #region ImportSysMethodReference
        protected MethodReference ImportSysMethodReference(ModuleDefinition syslib, ModuleDefinition target,
            string ns, string typeName, string method, bool isStatic, Type resType)
        {
            return ImportSysMethodReference(syslib, target, ns, typeName, method, isStatic, (Type[])null, resType);
        }

        protected MethodReference ImportSysMethodReference(ModuleDefinition syslib, ModuleDefinition target,
            string ns, string typeName, string method, bool isStatic, Type paramType, Type resType)
        {
            return ImportSysMethodReference(syslib, target, ns, typeName, method, isStatic, new Type[] { paramType }, resType);
        }

        protected MethodReference ImportSysMethodReference(ModuleDefinition syslib, ModuleDefinition target,
            string ns, string typeName, string method, bool isStatic, Type[] paramTypes, Type resType)
        {
            var methTypeRef = new TypeReference(ns, typeName, syslib, syslib);
            var resTypeRef = new TypeReference(resType.Namespace, resType.Name, syslib, syslib);
            var methRef = new MethodReference(method, resTypeRef, methTypeRef);
            if (!isStatic)
                methRef.HasThis = true;

            //parameters
            if (paramTypes != null)
            {
                for (var i = 0; i < paramTypes.Length; i++)
                {
                    var type = paramTypes[i];
                    if (type == null || type == typeof(void))
                        continue;
                    var parDef = CreateParameterDefinition($"par_{i}", type, syslib);
                    methRef.Parameters.Add(parDef);
                }
            }

            return target.ImportReference(methRef);
        }
        #endregion

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

        /// <summary>
        /// Get method reference to the system late binding call, e.g. _methInfo.Invoke(null, ...)
        /// </summary>
        /// <param name="syslib">System base lib getting by GetSysModule() call</param>
        /// <param name="target"></param>
        /// <returns></returns>
        protected MethodReference GetLateBindingInvoker(ModuleDefinition syslib, ModuleDefinition target)
        {
            var methInvoke = ImportSysMethodReference(syslib, target, "System.Reflection", "MethodBase", "Invoke", false,
                new Type[] { typeof(object), typeof(object[]) }, typeof(object));

            //HACK: for proper creating of object[] (and under/for the NetFx, and under/for the NetCore)
            var reflectRef = target.ImportReference(Cecilifier.Runtime.TypeHelpers.ResolveMethod("mscorlib", "System.Reflection.MethodBase", "Invoke", 
                System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, "", "System.Object", "System.Object[]"));
            methInvoke.Parameters.Clear();
            foreach (var p in reflectRef.Parameters)
                methInvoke.Parameters.Add(p);

            return methInvoke;
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
            var prgDir = GetProgDir(true);
            var dotnetDir = GetDotnetDir(prgDir);
            var prg32 = GetProgDir(false);
            if (!Directory.Exists(dotnetDir))
            {
                prgDir = prg32;
                dotnetDir = GetDotnetDir(prgDir);
            }
            //
            var root = isNetFx ?
                //ProgramFiles is prg32 for NetFx anyway
                Path.Combine(prg32, "Reference Assemblies", "Microsoft", "Framework", ".NETFramework") :
                Path.Combine(dotnetDir, "Microsoft.NETCore.App");
            var pattern = isNetFx ? "v4.*" : "5.*"; //TODO: for next versions
            var fileName = isNetFx ? "mscorlib.dll" : "System.Private.CoreLib.dll";
            var dirs = Directory.GetDirectories(root, pattern, SearchOption.TopDirectoryOnly)
                .Where(a => !a.Contains("X")) //NetFx folder without libs
                .OrderBy(a => a)
                .ToArray();
            if (dirs.Length == 0)
                throw new Exception("System lib's directory not found");
            var path = Path.Combine(dirs[^1], fileName);
            if (!File.Exists(path))
                throw new Exception("System lib not found");
            return path;
        }

        private string GetProgDir(bool is64)
        {
            return Environment.GetFolderPath(is64 ? Environment.SpecialFolder.ProgramFiles : Environment.SpecialFolder.ProgramFilesX86);
        }

        private string GetDotnetDir(string prgDir)
        {
            return Path.Combine(prgDir, "dotnet", "shared");
        }

        //TODO: full pattern!
        public virtual void Dispose()
        {
            foreach (var lib in _syslibs.Values)
                lib.Dispose();
        }
    }

    /*********************************************************************************************************/

    public abstract class AbstractCodeInjector<TOptions> : AbstractCodeInjector
        where TOptions : AbstractOptions
    {
        /// <summary>
        /// Inner options for executing the plugin
        /// </summary>
        public TOptions Options { get; set; }

        /*****************************************************************************************/

        protected string GetInnerConfigFullPath(string pathFromInjectorConfig)
        {
            if (string.IsNullOrWhiteSpace(pathFromInjectorConfig))
                return string.Empty;
            if (FileUtils.IsPossibleFilePath(pathFromInjectorConfig))
                return FileUtils.GetFullPath(pathFromInjectorConfig); //maybe it is relative path (will be used the Injector root)
            // it is just config name
            const string ext = ".yml";
            if (!pathFromInjectorConfig.EndsWith(ext))
                pathFromInjectorConfig += ext;
            return Path.Combine(FileUtils.EntryDir, pathFromInjectorConfig);
        }
    }
}
