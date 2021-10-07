﻿using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Cecilifier.Runtime;
using Drill4Net.Injector.Core;

namespace Drill4Net.Injection
{
    /// <summary>
    /// IL code generator for the injected Profiler's type
    /// </summary>
    public class ProfilerProxyInjector : AbstractCodeInjector, IProfilerProxyInjector
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
        public ProfilerProxyInjector(string proxyClass, string proxyFunc,
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
        }

        /*****************************************************************************************/

        /// <summary>
        /// Generating IL instructions for a class ProfilerProxy by Mono.Cecil
        /// </summary>
        /// <param name="assembly">The injected assembly</param>
        /// <param name="proxyNs">The Proxy's namespace for current assembly</param>
        /// <param name="isNetFX">Is NetFramework or Net Core?</param>
        public override void InjectTo(AssemblyDefinition assembly, string proxyNs, bool isNetFX = false)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            var module = assembly.MainModule;

            using var syslib = GetSysModule(isNetFX);

            #region ClassDeclaration : ProfilerProxy
            var t1 = new TypeDefinition(proxyNs, ProxyClass, TypeAttributes.AnsiClass | TypeAttributes.Public, module.TypeSystem.Object);
            module.Types.Add(t1);
            t1.BaseType = module.TypeSystem.Object;

            var fld_ProfilerProxy_methRegInfo = new FieldDefinition("_methRegInfo", FieldAttributes.Private | FieldAttributes.Static, ImportSysTypeReference(syslib, module, typeof(System.Reflection.MethodInfo)));
            t1.Fields.Add(fld_ProfilerProxy_methRegInfo);

            var fld_ProfilerProxy_methCmdInfo = new FieldDefinition("_methCmdInfo", FieldAttributes.Private | FieldAttributes.Static, ImportSysTypeReference(syslib, module, typeof(System.Reflection.MethodInfo)));
            t1.Fields.Add(fld_ProfilerProxy_methCmdInfo);
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

            #region _methRegInfo = type.GetMethod("Transmit");
            var Ldloc13 = il_ProfilerProxy_cctor_.Create(OpCodes.Ldloc, lv_type8);
            il_ProfilerProxy_cctor_.Append(Ldloc13);

            var methGetMethodRef = ImportSysMethodReference(syslib, module, "System", "Type", "GetMethod", false, typeof(string), typeof(System.Reflection.MethodInfo));
            //module.ImportReference(TypeHelpers.ResolveMethod(coreLib, "System.Type", "GetMethod", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, "", "System.String"));

            var Callvirt14 = il_ProfilerProxy_cctor_.Create(OpCodes.Callvirt, methGetMethodRef);
            var Ldstr15 = il_ProfilerProxy_cctor_.Create(OpCodes.Ldstr, ProfilerFunc);
            il_ProfilerProxy_cctor_.Append(Ldstr15);
            il_ProfilerProxy_cctor_.Append(Callvirt14);
            var Stsfld16 = il_ProfilerProxy_cctor_.Create(OpCodes.Stsfld, fld_ProfilerProxy_methRegInfo);
            il_ProfilerProxy_cctor_.Append(Stsfld16);
            #endregion
            #region _methRegInfo = type.GetMethod("DoCommand");
            //var Ldloc13 = il_ProfilerProxy_cctor_.Create(OpCodes.Ldloc, lv_type8);
            il_ProfilerProxy_cctor_.Append(Ldloc13);

            //var methGetMethodRef = ImportSysMethodReference(syslib, module, "System", "Type", "GetMethod", false, typeof(string), typeof(System.Reflection.MethodInfo));
            //module.ImportReference(TypeHelpers.ResolveMethod(coreLib, "System.Type", "GetMethod", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, "", "System.String"));

            //var Callvirt14 = il_ProfilerProxy_cctor_.Create(OpCodes.Callvirt, methGetMethodRef);
            var Ldstr152 = il_ProfilerProxy_cctor_.Create(OpCodes.Ldstr, "DoCommand");
            il_ProfilerProxy_cctor_.Append(Ldstr152);
            il_ProfilerProxy_cctor_.Append(Callvirt14);
            var Stsfld162 = il_ProfilerProxy_cctor_.Create(OpCodes.Stsfld, fld_ProfilerProxy_methCmdInfo);
            il_ProfilerProxy_cctor_.Append(Stsfld162);
            #endregion

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
            #region Method : Register
            var regMeth = new MethodDefinition(ProxyFunc, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, module.TypeSystem.Void);
            t1.Methods.Add(regMeth);
            regMeth.Body.InitLocals = true;
            var ilProc = regMeth.Body.GetILProcessor();

            //Parameters of 'public void Register(string data)'
            var data21 = new ParameterDefinition("data", ParameterAttributes.None, module.TypeSystem.String);
            regMeth.Parameters.Add(data21);

            //_methInfo.Invoke(null, new object[] { data });
            var Ldarg_022 = ilProc.Create(OpCodes.Nop);
            ilProc.Append(Ldarg_022);
            var Ldfld23 = ilProc.Create(OpCodes.Ldsfld, fld_ProfilerProxy_methRegInfo);
            ilProc.Append(Ldfld23);

            var methInvoke = ImportSysMethodReference(syslib, module, "System.Reflection", "MethodBase", "Invoke", false, new Type[] { typeof(object), typeof(object[]) }, typeof(object));
            //HACK: for proper creating of object[] (and under/for the NetFx, and under/for the NetCore)
            var reflectRef = module.ImportReference(TypeHelpers.ResolveMethod("mscorlib", "System.Reflection.MethodBase", "Invoke", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, "", "System.Object", "System.Object[]"));
            methInvoke.Parameters.Clear();
            foreach (var p in reflectRef.Parameters)
                methInvoke.Parameters.Add(p);

            var Callvirt24 = ilProc.Create(OpCodes.Callvirt, methInvoke);
            var Ldnull25 = ilProc.Create(OpCodes.Ldnull);
            ilProc.Append(Ldnull25);
            var Ldc_I426 = ilProc.Create(OpCodes.Ldc_I4, 1);
            ilProc.Append(Ldc_I426);
            var Newarr27 = ilProc.Create(OpCodes.Newarr, module.TypeSystem.Object);
            ilProc.Append(Newarr27);
            var Dup28 = ilProc.Create(OpCodes.Dup);
            ilProc.Append(Dup28);
            var Ldc_I429 = ilProc.Create(OpCodes.Ldc_I4, 0);
            ilProc.Append(Ldc_I429);
            var Ldarg_130 = ilProc.Create(OpCodes.Ldarg_0);
            ilProc.Append(Ldarg_130);
            var Stelem_Ref31 = ilProc.Create(OpCodes.Stelem_Ref);
            ilProc.Append(Stelem_Ref31);
            ilProc.Append(Callvirt24);
            var Pop32 = ilProc.Create(OpCodes.Pop);
            ilProc.Append(Pop32);

            //return
            var Ret33 = ilProc.Create(OpCodes.Ret);
            ilProc.Append(Ret33);
            #endregion
            #region Method : DoCommand
            var cmdMeth = new MethodDefinition("DoCommand", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, module.TypeSystem.Void);
            t1.Methods.Add(cmdMeth);
            cmdMeth.Body.InitLocals = true;
            ilProc = cmdMeth.Body.GetILProcessor();

            //Parameters of 'public void DoCommand(int command, string data)'
            data21 = new ParameterDefinition("command", ParameterAttributes.None, module.TypeSystem.Int32);
            cmdMeth.Parameters.Add(data21);

            data21 = new ParameterDefinition("data", ParameterAttributes.None, module.TypeSystem.String);
            cmdMeth.Parameters.Add(data21);

            //_meth.Invoke(null, new object[]{ command, data });
            var Ldsfld_6 = ilProc.Create(OpCodes.Ldsfld, fld_ProfilerProxy_methCmdInfo);
            ilProc.Append(Ldsfld_6);
            var Ldnull_9 = ilProc.Create(OpCodes.Ldnull);
            ilProc.Append(Ldnull_9);
            var Ldc_I4_10 = ilProc.Create(OpCodes.Ldc_I4, 2);
            ilProc.Append(Ldc_I4_10);
            var Newarr_11 = ilProc.Create(OpCodes.Newarr, assembly.MainModule.TypeSystem.Object);
            ilProc.Append(Newarr_11);
            var Dup_12 = ilProc.Create(OpCodes.Dup);
            ilProc.Append(Dup_12);
            var Ldc_I4_13 = ilProc.Create(OpCodes.Ldc_I4, 0);
            ilProc.Append(Ldc_I4_13);
            var Ldarg_0_14 = ilProc.Create(OpCodes.Ldarg_0);
            ilProc.Append(Ldarg_0_14);
            var Box_15 = ilProc.Create(OpCodes.Box, assembly.MainModule.TypeSystem.Int32);
            ilProc.Append(Box_15);
            var Stelem_Ref_16 = ilProc.Create(OpCodes.Stelem_Ref);
            ilProc.Append(Stelem_Ref_16);
            var Dup_17 = ilProc.Create(OpCodes.Dup);
            ilProc.Append(Dup_17);
            var Ldc_I4_18 = ilProc.Create(OpCodes.Ldc_I4, 1);
            ilProc.Append(Ldc_I4_18);
            var Ldarg_1_19 = ilProc.Create(OpCodes.Ldarg_1);
            ilProc.Append(Ldarg_1_19);
            var Stelem_Ref_20 = ilProc.Create(OpCodes.Stelem_Ref);
            ilProc.Append(Stelem_Ref_20);
            ilProc.Append(Callvirt24); //Callvirt_8

            Pop32 = ilProc.Create(OpCodes.Pop);
            ilProc.Append(Pop32);

            //return
            Ret33 = ilProc.Create(OpCodes.Ret);
            ilProc.Append(Ret33);
            #endregion

            //PrivateCoreLibFixer.FixReferences(assembly.MainModule); //it leads to fail in runtime
        }
    }
}