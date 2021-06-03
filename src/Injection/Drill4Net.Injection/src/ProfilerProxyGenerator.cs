using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Cecilifier.Runtime;

namespace Drill4Net.Injection
{
    public class ProfilerProxyGenerator
    {
        /* INFO *
            https://cecilifier.me/ - online translator C# to Mono.Cecil's instruction on C# (buggy and with restrictions!)
            on Github - https://github.com/adrianoc/cecilifier
        */

        #region Proxy
        public string ProxyNs { get; }
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

        public ProfilerProxyGenerator(string proxyNs,
									  string proxyClass,
									  string proxyFunc,

									  string profilerReadDir,
									  string profilerAsmName,
									  string profilerNs,
									  string profilerClass,
									  string profilerFunc)
        {
            ProxyNs = proxyNs ?? throw new ArgumentNullException(nameof(proxyNs));
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
        /// <param name="assembly"></param>
        /// <param name="isNetFX"></param>
        public void InjectTo(AssemblyDefinition assembly, bool isNetFX = false)
        {
			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));

			//TODO: check for NetFx!!!
			//mscorlib.dll / "netstandard" / "System.Runtime" - only System.Type, not Assembly
			var sysLib = isNetFX ? "System.Runtime" : "System.Private.CoreLib";
			var asmLib = isNetFX ? "System.Reflection" : "System.Private.CoreLib";

			#region ClassDeclaration : ProfilerProxy
			var t1 = new TypeDefinition(ProxyNs, ProxyClass, TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Public, assembly.MainModule.TypeSystem.Object);
			assembly.MainModule.Types.Add(t1);
			t1.BaseType = assembly.MainModule.TypeSystem.Object;

			var fld_ProfilerProxy__methInfo = new FieldDefinition("_methInfo", FieldAttributes.Private | FieldAttributes.Static, assembly.MainModule.ImportReference(typeof(System.Reflection.MethodInfo)));
			t1.Fields.Add(fld_ProfilerProxy__methInfo);
			#endregion
			#region Constructor : .cctor
			var ProfilerProxy_cctor_ = new MethodDefinition(".cctor", MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | MethodAttributes.HideBySig, assembly.MainModule.TypeSystem.Void);
			t1.Methods.Add(ProfilerProxy_cctor_);
			ProfilerProxy_cctor_.Body.InitLocals = true;
			var il_ProfilerProxy_cctor_ = ProfilerProxy_cctor_.Body.GetILProcessor();

			//var profPath = @"d:\Projects\EPM-D4J\!!_exp\Injector.Net\Agent.Test\bin\Debug\netstandard2.0\Agent.Test.dll";
			var lv_profPath1 = new VariableDefinition(assembly.MainModule.TypeSystem.String);
			ProfilerProxy_cctor_.Body.Variables.Add(lv_profPath1);
			var Ldstr2 = il_ProfilerProxy_cctor_.Create(OpCodes.Ldstr, $"{ProfilerReadDir}{ProfilerAsmName}");
			il_ProfilerProxy_cctor_.Append(Ldstr2);
			var Stloc3 = il_ProfilerProxy_cctor_.Create(OpCodes.Stloc, lv_profPath1);
			il_ProfilerProxy_cctor_.Append(Stloc3);

			//var asm = Assembly.LoadFrom(profPath);
			var lv_asm4 = new VariableDefinition(assembly.MainModule.ImportReference(typeof(System.Reflection.Assembly)));
			ProfilerProxy_cctor_.Body.Variables.Add(lv_asm4);
			var Call5 = il_ProfilerProxy_cctor_.Create(OpCodes.Call, assembly.MainModule.ImportReference(TypeHelpers.ResolveMethod(asmLib, "System.Reflection.Assembly", "LoadFrom", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, "", "System.String")));
			var Ldloc6 = il_ProfilerProxy_cctor_.Create(OpCodes.Ldloc, lv_profPath1);
			il_ProfilerProxy_cctor_.Append(Ldloc6);
			il_ProfilerProxy_cctor_.Append(Call5);
			var Stloc7 = il_ProfilerProxy_cctor_.Create(OpCodes.Stloc, lv_asm4);
			il_ProfilerProxy_cctor_.Append(Stloc7);

			//var type = asm.GetType("Agent.Tests.LoggerAgent");
			var lv_type8 = new VariableDefinition(assembly.MainModule.ImportReference(typeof(Type)));
			ProfilerProxy_cctor_.Body.Variables.Add(lv_type8);
			var Ldloc9 = il_ProfilerProxy_cctor_.Create(OpCodes.Ldloc, lv_asm4);
			il_ProfilerProxy_cctor_.Append(Ldloc9);
			var Callvirt10 = il_ProfilerProxy_cctor_.Create(OpCodes.Callvirt, assembly.MainModule.ImportReference(TypeHelpers.ResolveMethod(asmLib, "System.Reflection.Assembly", "GetType", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, "", "System.String")));
			var Ldstr11 = il_ProfilerProxy_cctor_.Create(OpCodes.Ldstr, $"{ProfilerNs}.{ProfilerClass}");
			il_ProfilerProxy_cctor_.Append(Ldstr11);
			il_ProfilerProxy_cctor_.Append(Callvirt10);
			var Stloc12 = il_ProfilerProxy_cctor_.Create(OpCodes.Stloc, lv_type8);
			il_ProfilerProxy_cctor_.Append(Stloc12);

			//_methInfo = type.GetMethod("Process");
			var Ldloc13 = il_ProfilerProxy_cctor_.Create(OpCodes.Ldloc, lv_type8);
			il_ProfilerProxy_cctor_.Append(Ldloc13);
			var Callvirt14 = il_ProfilerProxy_cctor_.Create(OpCodes.Callvirt, assembly.MainModule.ImportReference(TypeHelpers.ResolveMethod(sysLib, "System.Type", "GetMethod", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, "", "System.String")));
			var Ldstr15 = il_ProfilerProxy_cctor_.Create(OpCodes.Ldstr, ProfilerFunc);
			il_ProfilerProxy_cctor_.Append(Ldstr15);
			il_ProfilerProxy_cctor_.Append(Callvirt14);
			var Stsfld16 = il_ProfilerProxy_cctor_.Create(OpCodes.Stsfld, fld_ProfilerProxy__methInfo);
			il_ProfilerProxy_cctor_.Append(Stsfld16);
			var Ret17 = il_ProfilerProxy_cctor_.Create(OpCodes.Ret);
			il_ProfilerProxy_cctor_.Append(Ret17);
            #endregion
            #region Constructor: .ctor
            var ProfilerProxy_ctor_ = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName, assembly.MainModule.TypeSystem.Void);
			t1.Methods.Add(ProfilerProxy_ctor_);
			var il18 = ProfilerProxy_ctor_.Body.GetILProcessor();
			var Ldarg_019 = il18.Create(OpCodes.Ldarg_0);
			il18.Append(Ldarg_019);
			var Call20 = il18.Create(OpCodes.Call, assembly.MainModule.ImportReference(TypeHelpers.DefaultCtorFor(t1.BaseType)));
			il18.Append(Call20);
			var Ret21 = il18.Create(OpCodes.Ret);
			il18.Append(Ret21);
			#endregion
			#region Method : Process
			var ProfilerProxy_Process_string = new MethodDefinition(ProxyFunc, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, assembly.MainModule.TypeSystem.Void);
			t1.Methods.Add(ProfilerProxy_Process_string);
			ProfilerProxy_Process_string.Body.InitLocals = true;
			var il_ProfilerProxy_Process_string = ProfilerProxy_Process_string.Body.GetILProcessor();

			//Parameters of 'public void Process(string data)'
			var data21 = new ParameterDefinition("data", ParameterAttributes.None, assembly.MainModule.TypeSystem.String);
			ProfilerProxy_Process_string.Parameters.Add(data21);

			//_methInfo.Invoke(null, new object[] { data });
			var Ldarg_022 = il_ProfilerProxy_Process_string.Create(OpCodes.Nop);
			il_ProfilerProxy_Process_string.Append(Ldarg_022);
			var Ldfld23 = il_ProfilerProxy_Process_string.Create(OpCodes.Ldsfld, fld_ProfilerProxy__methInfo);
			il_ProfilerProxy_Process_string.Append(Ldfld23);
			var Callvirt24 = il_ProfilerProxy_Process_string.Create(OpCodes.Callvirt, assembly.MainModule.ImportReference(TypeHelpers.ResolveMethod(asmLib, "System.Reflection.MethodBase", "Invoke", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, "", "System.Object", "System.Object[]")));
			var Ldnull25 = il_ProfilerProxy_Process_string.Create(OpCodes.Ldnull);
			il_ProfilerProxy_Process_string.Append(Ldnull25);
			var Ldc_I426 = il_ProfilerProxy_Process_string.Create(OpCodes.Ldc_I4, 1);
			il_ProfilerProxy_Process_string.Append(Ldc_I426);
			var Newarr27 = il_ProfilerProxy_Process_string.Create(OpCodes.Newarr, assembly.MainModule.TypeSystem.Object);
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
    }
}