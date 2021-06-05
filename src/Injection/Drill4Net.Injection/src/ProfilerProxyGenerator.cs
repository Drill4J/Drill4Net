using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Cecilifier.Runtime;
using Drill4Net.Injector.Core;

namespace Drill4Net.Injection
{
    /// <summary>
    /// IL code generator for the injected Profiler's type
    /// </summary>
    public class ProfilerProxyGenerator : IProfilerProxyGenerator
    {
        /* INFO *
            https://cecilifier.me/ - online translator C# to Mono.Cecil's instruction on C# (buggy and with restrictions!)
            on Github - https://github.com/adrianoc/cecilifier
        */

        #region Proxy
        public string ProxyClass { get; }
        public string ProxyFunc { get; }
        #endregion
        #region Real
        public string ProfilerReadDir { get; }
        public string ProfilerAsmName { get; }
        public string ProfilerNs { get; }
        public string ProfilerClass { get; }
        public string ProfilerFunc { get; }
        #endregion

        private readonly Dictionary<bool, ModuleDefinition> _syslibs;

        /*****************************************************************************************/

        /// <summary>
        /// Create the IL code generator for the injected Profiler's type
        /// </summary>
        /// <param name="proxyClass"></param>
        /// <param name="proxyFunc"></param>
        /// <param name="profilerReadDir"></param>
        /// <param name="profilerAsmName"></param>
        /// <param name="profilerNs"></param>
        /// <param name="profilerClass"></param>
        /// <param name="profilerFunc"></param>
        public ProfilerProxyGenerator(string proxyClass, string proxyFunc,
                                      string profilerReadDir, string profilerAsmName,
                                      string profilerNs, string profilerClass, string profilerFunc)
        {
            ProxyClass = proxyClass ?? throw new ArgumentNullException(nameof(proxyClass));
            ProxyFunc = proxyFunc ?? throw new ArgumentNullException(nameof(proxyFunc));

            ProfilerReadDir = profilerReadDir ?? throw new ArgumentNullException(nameof(profilerReadDir));
            ProfilerAsmName = profilerAsmName ?? throw new ArgumentNullException(nameof(profilerAsmName));
            ProfilerNs = profilerNs ?? throw new ArgumentNullException(nameof(profilerNs));
            ProfilerClass = profilerClass ?? throw new ArgumentNullException(nameof(profilerClass));
            ProfilerFunc = profilerFunc ?? throw new ArgumentNullException(nameof(profilerFunc));

            _syslibs = new Dictionary<bool, ModuleDefinition>();
        }

        /*****************************************************************************************/

        /// <summary>
        /// Generating IL instructions for a class ProfilerProxy by Mono.Cecil
        /// </summary>
        /// <param name="assembly">The injected assembly</param>
        /// <param name="proxyNs">The Proxy's namespace for current assembly</param>
        /// <param name="isNetFX">Is NetFramework or Net Core?</param>
        public void InjectTo(AssemblyDefinition assembly, string proxyNs, bool isNetFX = false)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            var module = assembly.MainModule;

            using var syslib = GetSysModule(isNetFX);

            #region ClassDeclaration : ProfilerProxy
            var t1 = new TypeDefinition(proxyNs, ProxyClass, TypeAttributes.AnsiClass | TypeAttributes.Public, module.TypeSystem.Object);
            module.Types.Add(t1);
            t1.BaseType = module.TypeSystem.Object;

            var fld_ProfilerProxy__methInfo = new FieldDefinition("_methInfo", FieldAttributes.Private | FieldAttributes.Static, ImportSysTypeReference(syslib, module, typeof(System.Reflection.MethodInfo)));
            t1.Fields.Add(fld_ProfilerProxy__methInfo);
            #endregion
            #region Constructor : .cctor
            var ProfilerProxy_cctor_ = new MethodDefinition(".cctor", MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | MethodAttributes.HideBySig, module.TypeSystem.Void);
            t1.Methods.Add(ProfilerProxy_cctor_);
            ProfilerProxy_cctor_.Body.InitLocals = true;
            var il_ProfilerProxy_cctor_ = ProfilerProxy_cctor_.Body.GetILProcessor();

            //var profPath = @"d:\Projects\EPM-D4J\!!_exp\Injector.Net\Agent.Test\bin\Debug\netstandard2.0\Agent.Test.dll";
            var lv_profPath1 = new VariableDefinition(module.TypeSystem.String);
            ProfilerProxy_cctor_.Body.Variables.Add(lv_profPath1);
            var Ldstr2 = il_ProfilerProxy_cctor_.Create(OpCodes.Ldstr, $"{ProfilerReadDir}{ProfilerAsmName}");
            il_ProfilerProxy_cctor_.Append(Ldstr2);
            var Stloc3 = il_ProfilerProxy_cctor_.Create(OpCodes.Stloc, lv_profPath1);
            il_ProfilerProxy_cctor_.Append(Stloc3);

            //var asm = Assembly.LoadFrom(profPath);
            var lv_asm4 = new VariableDefinition(ImportSysTypeReference(syslib, module, typeof(System.Reflection.Assembly)));
            ProfilerProxy_cctor_.Body.Variables.Add(lv_asm4);

            var methLoadFrom = ImportSysMethodReference(syslib, module, "System.Reflection", "Assembly", "LoadFrom", true, typeof(string), typeof(System.Reflection.Assembly));
            //module.ImportReference(TypeHelpers.ResolveMethod(coreLib, "System.Reflection.Assembly", "LoadFrom", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, "", "System.String"));

            var Call5 = il_ProfilerProxy_cctor_.Create(OpCodes.Call, methLoadFrom);
            var Ldloc6 = il_ProfilerProxy_cctor_.Create(OpCodes.Ldloc, lv_profPath1);
            il_ProfilerProxy_cctor_.Append(Ldloc6);
            il_ProfilerProxy_cctor_.Append(Call5);
            var Stloc7 = il_ProfilerProxy_cctor_.Create(OpCodes.Stloc, lv_asm4);
            il_ProfilerProxy_cctor_.Append(Stloc7);

            //var type = asm.GetType("Agent.Tests.LoggerAgent");
            var lv_type8 = new VariableDefinition(ImportSysTypeReference(syslib, module, typeof(Type)));
            ProfilerProxy_cctor_.Body.Variables.Add(lv_type8);
            var Ldloc9 = il_ProfilerProxy_cctor_.Create(OpCodes.Ldloc, lv_asm4);
            il_ProfilerProxy_cctor_.Append(Ldloc9);

            var methGetType = ImportSysMethodReference(syslib, module, "System.Reflection", "Assembly", "GetType", false, typeof(string), typeof(Type));
            //module.ImportReference(TypeHelpers.ResolveMethod(coreLib, "System.Reflection.Assembly", "GetType", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, "", "System.String"));

            var Callvirt10 = il_ProfilerProxy_cctor_.Create(OpCodes.Callvirt, methGetType);
            var Ldstr11 = il_ProfilerProxy_cctor_.Create(OpCodes.Ldstr, $"{ProfilerNs}.{ProfilerClass}");
            il_ProfilerProxy_cctor_.Append(Ldstr11);
            il_ProfilerProxy_cctor_.Append(Callvirt10);
            var Stloc12 = il_ProfilerProxy_cctor_.Create(OpCodes.Stloc, lv_type8);
            il_ProfilerProxy_cctor_.Append(Stloc12);

            //_methInfo = type.GetMethod("Process");
            var Ldloc13 = il_ProfilerProxy_cctor_.Create(OpCodes.Ldloc, lv_type8);
            il_ProfilerProxy_cctor_.Append(Ldloc13);

            var methGetMethodRef = ImportSysMethodReference(syslib, module, "System", "Type", "GetMethod", false, typeof(string), typeof(System.Reflection.MethodInfo));
            //module.ImportReference(TypeHelpers.ResolveMethod(coreLib, "System.Type", "GetMethod", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, "", "System.String"));

            var Callvirt14 = il_ProfilerProxy_cctor_.Create(OpCodes.Callvirt, methGetMethodRef);
            var Ldstr15 = il_ProfilerProxy_cctor_.Create(OpCodes.Ldstr, ProfilerFunc);
            il_ProfilerProxy_cctor_.Append(Ldstr15);
            il_ProfilerProxy_cctor_.Append(Callvirt14);
            var Stsfld16 = il_ProfilerProxy_cctor_.Create(OpCodes.Stsfld, fld_ProfilerProxy__methInfo);
            il_ProfilerProxy_cctor_.Append(Stsfld16);

            var Ret17 = il_ProfilerProxy_cctor_.Create(OpCodes.Ret);
            il_ProfilerProxy_cctor_.Append(Ret17);
            #endregion
            #region Constructor: .ctor
            var ProfilerProxy_ctor_ = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName, module.TypeSystem.Void);
            t1.Methods.Add(ProfilerProxy_ctor_);
            var il18 = ProfilerProxy_ctor_.Body.GetILProcessor();
            var Ldarg_019 = il18.Create(OpCodes.Ldarg_0);
            il18.Append(Ldarg_019);
            var Call20 = il18.Create(OpCodes.Call, module.ImportReference(TypeHelpers.DefaultCtorFor(t1.BaseType)));
            il18.Append(Call20);
            var Ret21 = il18.Create(OpCodes.Ret);
            il18.Append(Ret21);
            #endregion
            #region Method : Process
            var ProfilerProxy_Process_string = new MethodDefinition(ProxyFunc, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, module.TypeSystem.Void);
            t1.Methods.Add(ProfilerProxy_Process_string);
            ProfilerProxy_Process_string.Body.InitLocals = true;
            var il_ProfilerProxy_Process_string = ProfilerProxy_Process_string.Body.GetILProcessor();

            //Parameters of 'public void Process(string data)'
            var data21 = new ParameterDefinition("data", ParameterAttributes.None, module.TypeSystem.String);
            ProfilerProxy_Process_string.Parameters.Add(data21);

            //_methInfo.Invoke(null, new object[] { data });
            var Ldarg_022 = il_ProfilerProxy_Process_string.Create(OpCodes.Nop);
            il_ProfilerProxy_Process_string.Append(Ldarg_022);
            var Ldfld23 = il_ProfilerProxy_Process_string.Create(OpCodes.Ldsfld, fld_ProfilerProxy__methInfo);
            il_ProfilerProxy_Process_string.Append(Ldfld23);

            var methInvoke = ImportSysMethodReference(syslib, module, "System.Reflection", "MethodBase", "Invoke", false, new Type[] { typeof(object), typeof(object[]) }, typeof(object));
            //HACK: for proper creating of object[] (and under/for the NetFx, and under/for the NetCore)
            var reflectRef = module.ImportReference(TypeHelpers.ResolveMethod("mscorlib", "System.Reflection.MethodBase", "Invoke", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, "", "System.Object", "System.Object[]"));
            methInvoke.Parameters.Clear();
            foreach (var p in reflectRef.Parameters)
                methInvoke.Parameters.Add(p);

            var Callvirt24 = il_ProfilerProxy_Process_string.Create(OpCodes.Callvirt, methInvoke);
            var Ldnull25 = il_ProfilerProxy_Process_string.Create(OpCodes.Ldnull);
            il_ProfilerProxy_Process_string.Append(Ldnull25);
            var Ldc_I426 = il_ProfilerProxy_Process_string.Create(OpCodes.Ldc_I4, 1);
            il_ProfilerProxy_Process_string.Append(Ldc_I426);
            var Newarr27 = il_ProfilerProxy_Process_string.Create(OpCodes.Newarr, module.TypeSystem.Object);
            il_ProfilerProxy_Process_string.Append(Newarr27);
            var Dup28 = il_ProfilerProxy_Process_string.Create(OpCodes.Dup);
            il_ProfilerProxy_Process_string.Append(Dup28);
            var Ldc_I429 = il_ProfilerProxy_Process_string.Create(OpCodes.Ldc_I4, 0);
            il_ProfilerProxy_Process_string.Append(Ldc_I429);
            var Ldarg_130 = il_ProfilerProxy_Process_string.Create(OpCodes.Ldarg_0);
            il_ProfilerProxy_Process_string.Append(Ldarg_130);
            var Stelem_Ref31 = il_ProfilerProxy_Process_string.Create(OpCodes.Stelem_Ref);
            il_ProfilerProxy_Process_string.Append(Stelem_Ref31);
            il_ProfilerProxy_Process_string.Append(Callvirt24);
            var Pop32 = il_ProfilerProxy_Process_string.Create(OpCodes.Pop);
            il_ProfilerProxy_Process_string.Append(Pop32);

            //return
            var Ret33 = il_ProfilerProxy_Process_string.Create(OpCodes.Ret);
            il_ProfilerProxy_Process_string.Append(Ret33);
            #endregion

            //PrivateCoreLibFixer.FixReferences(assembly.MainModule); //it leads to fail in runtime
        }

        internal MethodReference ImportSysMethodReference(ModuleDefinition syslib, ModuleDefinition target, string ns, string typeName, string method, bool isStatic,
            Type parType, Type resType)
        {
            return ImportSysMethodReference(syslib, target, ns, typeName, method, isStatic, new Type[] { parType }, resType);
        }

        internal MethodReference ImportSysMethodReference(ModuleDefinition syslib, ModuleDefinition target, string ns, string typeName, string method, bool isStatic,
            Type[] parTypes, Type resType)
        {
            var methTypeRef = new TypeReference(ns, typeName, syslib, syslib);
            var restTypeRef = new TypeReference(resType.Namespace, resType.Name, syslib, syslib);
            var methRef = new MethodReference(method, restTypeRef, methTypeRef);
            if (!isStatic)
                methRef.HasThis = true;

            //parameters
            for (int i = 0; i < parTypes.Length; i++)
            {
                var parType = parTypes[i];
                var parTypeRef = new TypeReference(parType.Namespace, parType.Name, syslib, syslib);
                var parDef = new ParameterDefinition($"par_{i}", ParameterAttributes.None, parTypeRef);
                methRef.Parameters.Add(parDef);
            }

            return target.ImportReference(methRef);
        }

        internal TypeReference ImportSysTypeReference(ModuleDefinition syslib, ModuleDefinition target, Type type)
        {
            return target.ImportReference(new TypeReference(type.Namespace, type.Name, syslib, syslib));
        }

        internal ModuleDefinition GetSysModule(bool isNetFx)
        {
            if (_syslibs.ContainsKey(isNetFx))
                return _syslibs[isNetFx];
            var module = ModuleDefinition.ReadModule(GetSysLibPath(isNetFx), new ReaderParameters());
            _syslibs.Add(isNetFx, module);
            return module;
        }

        internal string GetSysLibPath(bool isNetFx)
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
            if (!dirs.Any())
                throw new Exception("System lib's directory not found");
            var path = Path.Combine(dirs[dirs.Length - 1], fileName);
            if (!File.Exists(path))
                throw new Exception("System lib not found");
            return path;
        }

        public void Dispose()
        {
            foreach (var lib in _syslibs.Values)
                lib.Dispose();
        }
    }
}