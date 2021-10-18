using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Drill4Net.Injector.Core;
using Drill4Net.Agent.Abstract;
using Cecilifier.Runtime;

namespace Drill4Net.Injection.SpecFlow
{
    public class SpecFlowHookInjector : AbstractCodeInjector
    {
        public string SourceDir { get; }
        public string ProxyClass { get; }

        private const string SPEC_NS = "TechTalk.SpecFlow";
        private ModuleDefinition _speclib;

        /*************************************************************************************************/

        public SpecFlowHookInjector(string sourceDir, string proxyClass): base(null)
        {
            SourceDir = sourceDir ?? throw new ArgumentNullException(nameof(sourceDir));
            ProxyClass = proxyClass ?? throw new ArgumentNullException(nameof(proxyClass));
            LoadTestFramework(sourceDir);
        }

        /*************************************************************************************************/

        private void LoadTestFramework(string sourceDir)
        {
            const string dllName = "TechTalk.SpecFlow.dll";
            var specDir = Path.Combine(Common.FileUtils.GetExecutionDir(), dllName);
            if (!File.Exists(specDir))
                specDir = Path.Combine(sourceDir, dllName);
            if (!File.Exists(specDir))
                throw new FileNotFoundException("SpecFlow framework is not found");
            _speclib = ModuleDefinition.ReadModule(specDir, new ReaderParameters());
        }

        //.custom instance void [TechTalk.SpecFlow]TechTalk.SpecFlow.BeforeScenarioAttribute::.ctor(string[]) = (
        //    01 00 00 00 00 00 01 00 54 08 05 4f 72 64 65 72
        //    00 00 00 00

        //IL_0000: nop
        //// DemoTransmitter.DoCommand(2, scenarioContext.ScenarioInfo.Title);
        //IL_0001: ldc.i4.2
        //IL_0002: ldarg.0
        //IL_0003: callvirt instance class [TechTalk.SpecFlow] TechTalk.SpecFlow.ScenarioInfo[TechTalk.SpecFlow] TechTalk.SpecFlow.ScenarioContext::get_ScenarioInfo()
        //IL_0008: callvirt instance string[TechTalk.SpecFlow] TechTalk.SpecFlow.ScenarioInfo::get_Title()
        //IL_000d: call void Drill4Net.Injection.SpecFlow.DemoTransmitter::DoCommand(int32, string) //will be differ in [assembly] namespace.type
        //IL_0012: nop
        //IL_0013: ret

        public override void InjectTo(AssemblyDefinition assembly, string proxyNs, bool isNetFX = false)
        {
            var type = GetClassTypeWithBindingAttribute(assembly);
            if (type == null)
                return;
            //
            var module = assembly.MainModule;
            //InjectInitMethod(module, type, ...);
            InjectContextDataInvoker(module, type, isNetFX);
            //
            //InjectHook(module, type, proxyNs, typeof(TechTalk.SpecFlow.BeforeFeatureAttribute), "FeatureContext", "FeatureInfo", "Drill4NetFeatureStarting", 0, isNetFX);
            //InjectHook(module, type, proxyNs, typeof(TechTalk.SpecFlow.AfterFeatureAttribute), "FeatureContext", "FeatureInfo", "Drill4NetFeatureFinished", 1, isNetFX);
            InjectHook(module, type, proxyNs, typeof(TechTalk.SpecFlow.BeforeScenarioAttribute), "ScenarioContext", "ScenarioInfo", "Drill4NetScenarioStarting", (int)AgentCommandType.TEST_CASE_START, isNetFX);
            InjectHook(module, type, proxyNs, typeof(TechTalk.SpecFlow.AfterScenarioAttribute), "ScenarioContext", "ScenarioInfo", "Drill4NetScenarioFinished", (int)AgentCommandType.TEST_CASE_STOP, isNetFX);
        }

        private TypeDefinition GetClassTypeWithBindingAttribute(AssemblyDefinition assembly)
        {
            var attr = assembly.CustomAttributes.Where(a => a.AttributeType.Name == "TechTalk.SpecFlow.xUnit.SpecFlowPlugin.AssemblyFixtureAttribute");
            if (attr == null)
                return null;
            var type = assembly.MainModule.Types //need just first type
                .FirstOrDefault(a => a.CustomAttributes.Any(b => b.AttributeType.FullName == "TechTalk.SpecFlow.BindingAttribute"));
            return type;
        }

        private void InjectInitMethod(ModuleDefinition module, TypeDefinition type, string proxyNs, Type methAttrType,
                string paramCtxType, string paramInfoType, bool isNetFX)
        {
            
        }

        private void InjectContextDataInvoker(ModuleDefinition module, TypeDefinition classType, bool isNetFX)
        {
            var syslib = GetSysModule(isNetFX); //inner caching & disposing
            var assembly = module.Assembly;

           //Method : GetContextData
           var m_GetContextData_2 = new MethodDefinition("GetContextData", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, assembly.MainModule.TypeSystem.Void);
            m_GetContextData_2.ReturnType = assembly.MainModule.TypeSystem.String;
            classType.Methods.Add(m_GetContextData_2);
            m_GetContextData_2.Body.InitLocals = true;
            var ilProc = m_GetContextData_2.Body.GetILProcessor();

            //Parameters of 'public static string GetContextData(MethodInfo meth, object featureCtx, object scenarioCtx)'
            var p_meth_4 = new ParameterDefinition("meth", ParameterAttributes.None, assembly.MainModule.ImportReference(typeof(System.Reflection.MethodInfo)));
            m_GetContextData_2.Parameters.Add(p_meth_4);
            var p_featureCtx_5 = new ParameterDefinition("featureCtx", ParameterAttributes.None, assembly.MainModule.TypeSystem.Object);
            m_GetContextData_2.Parameters.Add(p_featureCtx_5);
            var p_scenarioCtx_6 = new ParameterDefinition("scenarioCtx", ParameterAttributes.None, assembly.MainModule.TypeSystem.Object);
            m_GetContextData_2.Parameters.Add(p_scenarioCtx_6);

            //return meth.Invoke(null,
                //new object[]
                //{
                //featureCtx,
                //scenarioCtx,
                //Assembly.GetExecutingAssembly().Location,
            //}).ToString();
            ilProc.Emit(OpCodes.Ldarg_0);
            ilProc.Emit(OpCodes.Ldnull);
            ilProc.Emit(OpCodes.Ldc_I4, 3);
            ilProc.Emit(OpCodes.Newarr, assembly.MainModule.TypeSystem.Object);
            ilProc.Emit(OpCodes.Dup);
            ilProc.Emit(OpCodes.Ldc_I4, 0);
            ilProc.Emit(OpCodes.Ldarg_1);
            ilProc.Emit(OpCodes.Stelem_Ref);
            ilProc.Emit(OpCodes.Dup);
            ilProc.Emit(OpCodes.Ldc_I4, 1);
            ilProc.Emit(OpCodes.Ldarg_2);
            ilProc.Emit(OpCodes.Stelem_Ref);
            ilProc.Emit(OpCodes.Dup);
            ilProc.Emit(OpCodes.Ldc_I4, 2);

            //var m_execAsmMeth = assembly.MainModule.ImportReference(TypeHelpers.ResolveMethod("System.Private.CoreLib", "System.Reflection.Assembly", "GetExecutingAssembly", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, ""));
            var m_execAsmMeth = ImportSysMethodReference(syslib, module, "System.Reflection", "Assembly", "GetExecutingAssembly", true, typeof(System.Reflection.Assembly));
            ilProc.Emit(OpCodes.Call, m_execAsmMeth);

            //var m_locationMeth = assembly.MainModule.ImportReference(TypeHelpers.ResolveMethod("System.Private.CoreLib", "System.Reflection.Assembly", "get_Location", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, ""));
            var m_locationMeth = ImportSysMethodReference(syslib, module, "System.Reflection", "Assembly", "get_Location", false, typeof(string));
            ilProc.Emit(OpCodes.Callvirt, m_locationMeth);
            ilProc.Emit(OpCodes.Stelem_Ref);

            //meth.Invoke(null, ...)
            var methInvoke = GetLateBindingInvoker(syslib, module);
            ilProc.Emit(OpCodes.Callvirt, methInvoke);

            //var m_toStringMeth = assembly.MainModule.ImportReference(TypeHelpers.ResolveMethod("System.Private.CoreLib", "System.Object", "ToString", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, ""));
            var m_toStringMeth = ImportSysMethodReference(syslib, module, "System", "Object", "ToString", false, typeof(string));
            ilProc.Emit(OpCodes.Callvirt, m_toStringMeth);
            ilProc.Emit(OpCodes.Ret);
        }

        private void InjectHook(ModuleDefinition module, TypeDefinition type, string proxyNs, Type methAttrType,
                                string paramCtxType, string paramInfoType, string funcName, int command, bool isNetFX)
        {
            var syslib = GetSysModule(isNetFX); //inner caching & disposing
            var cmdMethodName = "DoCommand";

            //Drill4NetScenarioStarting
            //var funcName = "Drill4NetScenarioStarting";
            var funcDef = new MethodDefinition(funcName, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, module.TypeSystem.Void);
            type.Methods.Add(funcDef);

            AddOrder0Attribute(module, methAttrType, funcDef);

            funcDef.Body.InitLocals = true; //?
            var ilProc = funcDef.Body.GetILProcessor();

            //Parameters of 'public static void Drill4NetScenarioStarting(ScenarioContext scenarioContext)'
            var methParamRef = module.ImportReference(new TypeReference(SPEC_NS, paramCtxType, _speclib, _speclib));
            var par = new ParameterDefinition("context", ParameterAttributes.None, methParamRef);
            funcDef.Parameters.Add(par);

            ilProc.Append(ilProc.Create(OpCodes.Nop));
            ilProc.Append(ilProc.Create(OpCodes.Ldc_I4, command));
            ilProc.Append(ilProc.Create(OpCodes.Ldarg_0));

            //get_ScenarioInfo
            var typeRef1 = module.ImportReference(new TypeReference(SPEC_NS, paramCtxType, _speclib, _speclib));
            var resTypeRef1 = module.ImportReference(new TypeReference(SPEC_NS, paramInfoType, _speclib, _speclib));
            var methInfo1 = new MethodReference($"get_{paramInfoType}", resTypeRef1, typeRef1);
            methInfo1.HasThis = true;
            ilProc.Append(ilProc.Create(OpCodes.Callvirt, methInfo1));

            //get_Title
            var typeRef2 = module.ImportReference(new TypeReference(SPEC_NS, paramInfoType, _speclib, _speclib));
            var resTypeRef2 = module.TypeSystem.String;
            var methInfo2 = new MethodReference("get_Title", resTypeRef2, typeRef2);
            methInfo2.HasThis = true;
            ilProc.Append(ilProc.Create(OpCodes.Callvirt, methInfo2));

            //Invoke
            var typeRef3 = new TypeReference(proxyNs, ProxyClass, module, module); //located in the same assembly
            var methInfo3 = new MethodReference(cmdMethodName, module.TypeSystem.Void, typeRef3);
            //par1
            var parDef1 = ImportParameterDefinition("command", typeof(int), syslib, module);
            methInfo3.Parameters.Add(parDef1);
            //par2
            var parDef2 = ImportParameterDefinition("data", typeof(string), syslib, module);
            methInfo3.Parameters.Add(parDef2);

            ilProc.Append(ilProc.Create(OpCodes.Call, methInfo3));

            ilProc.Append(ilProc.Create(OpCodes.Nop));
            ilProc.Append(ilProc.Create(OpCodes.Ret));
        }

        internal void AddOrder0Attribute(ModuleDefinition module, Type attribType,
                ICustomAttributeProvider targetMember) // Can be a PropertyDefinition, MethodDefinition or other member definitions
        {
            // ctor
            var constructor = attribType.GetConstructors()[0];
            var constructorRef = module.ImportReference(constructor);
            var attrib = new CustomAttribute(constructorRef);
            // The argument
            var arg = new CustomAttributeArgument(module.ImportReference(typeof(int)), 0);
            attrib.ConstructorArguments.Add(arg);
            attrib.Properties.Add(new CustomAttributeNamedArgument("Order", arg));
            targetMember.CustomAttributes.Add(attrib);
        }

        //TODO: full pattern!
        public override void Dispose()
        {
            _speclib.Dispose();
            _speclib = null;
            base.Dispose();
        }
    }
}
