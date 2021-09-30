using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Drill4Net.Injector.Core;

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
            //
            var specDir = Path.Combine(Common.FileUtils.GetExecutionDir(), "TechTalk.SpecFlow.dll");
            _speclib = ModuleDefinition.ReadModule(specDir, new ReaderParameters());
        }

        /*************************************************************************************************/

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
            var attr = assembly.CustomAttributes.Where(a => a.AttributeType.Name == "TechTalk.SpecFlow.xUnit.SpecFlowPlugin.AssemblyFixtureAttribute");
            if (attr == null)
                return;
            var module = assembly.MainModule;
            var type = module.Types //need just first type
                .FirstOrDefault(a => a.CustomAttributes.Any(b => b.AttributeType.FullName == "TechTalk.SpecFlow.BindingAttribute"));
            if (type == null)
                return;
            //
            InjectMethod(module, type, proxyNs, typeof(TechTalk.SpecFlow.BeforeFeatureAttribute), "FeatureContext", "FeatureInfo", "Drill4NetFeatureStarting", 0, isNetFX);
            InjectMethod(module, type, proxyNs, typeof(TechTalk.SpecFlow.AfterFeatureAttribute), "FeatureContext", "FeatureInfo", "Drill4NetFeatureFinishing", 1, isNetFX);
            InjectMethod(module, type, proxyNs, typeof(TechTalk.SpecFlow.BeforeScenarioAttribute), "ScenarioContext", "ScenarioInfo", "Drill4NetScenarioStarting", 2, isNetFX);
            InjectMethod(module, type, proxyNs, typeof(TechTalk.SpecFlow.AfterScenarioAttribute), "ScenarioContext", "ScenarioInfo", "Drill4NetScenarioFinishing", 3, isNetFX);
        }

        private void InjectMethod(ModuleDefinition module, TypeDefinition type, string proxyNs, Type methAttrType,
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
