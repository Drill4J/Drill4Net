﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using YamlDotNet.Serialization;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Injector.Core;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Transmitter.SpecFlow;

namespace Drill4Net.Injection.SpecFlow
{
    /// <summary>
    /// Injector for SpecFlow BDD framework's specific (workflow hooks, etc)
    /// </summary>
    public class SpecFlowHookInjector : AbstractCodeInjector<SpecFlowPluginOptions>, IInjectorPlugin
    {
        public string Name => PluginName;

        public string SourceDir { get; }
        public string ProxyClass { get; }
        public string HelperReadDir { get; }
        public string HelperClass { get; }
        public string HelperNs { get; }
        public string HelperAsmName { get; }

        public const string PluginName = "SpecFlow";

        private const string _getContextDataMethod = "GetContextData";
        private const string _scenarioField = "_scenarioMethInfo";
        private const string SPEC_NS = "TechTalk.SpecFlow";

        private IProfilerProxyInjector _proxyGenerator;
        private readonly PluginLoaderOptions _loaderOpts;
        private readonly IDeserializer _deser;
        private ModuleDefinition _speclib;
        private readonly Logger _logger;

        /*************************************************************************************************/

        public SpecFlowHookInjector(string sourceDir, string proxyClass, PluginLoaderOptions loaderCfg)
        {
            _logger = new TypedLogger<SpecFlowHookInjector>(CoreConstants.SUBSYSTEM_INJECTOR_PLUGIN);

            SourceDir = sourceDir ?? throw new ArgumentNullException(nameof(sourceDir));
            ProxyClass = proxyClass ?? throw new ArgumentNullException(nameof(proxyClass));
            _loaderOpts = loaderCfg ?? throw new ArgumentNullException(nameof(loaderCfg));

            HelperReadDir = loaderCfg.Path;

            // these are real constants, aren't the cfg params
            var typeHelper = typeof(SpecFlowGeneratorContexter);
            HelperClass = typeHelper.Name;
            HelperNs = typeHelper.Namespace;
            HelperAsmName = Path.GetFileName(typeHelper.Assembly.Location); // "Drill4Net.Agent.Transmitter.SpecFlow.dll";

            _deser = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();
            Options = GetOptions(loaderCfg.Config);

            LoadTestFramework(sourceDir);
        }

        /*************************************************************************************************/

        public SpecFlowPluginOptions GetOptions(string plugConfigPath)
        {
            var configPath = GetInnerConfigFullPath(plugConfigPath);
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"File for plugin's options not found: [{configPath}]. Path from Injector parameters was: [{plugConfigPath}]");
            var cfgStr = File.ReadAllText(configPath);
            return _deser.Deserialize<SpecFlowPluginOptions>(cfgStr);
        }

        #region Traverse the assemblies
        public async Task Process(RunContext runCtx)
        {
            if(runCtx == null)
                throw new ArgumentNullException(nameof(runCtx));
            _proxyGenerator = runCtx.ProxyGenerator;

            //inner folders: already filtered by base process needed targets' directories
            var dirs = runCtx.MonikerDirectories.ToList(); //copy
            if (dirs.Count == 0)
                dirs.Add(runCtx.Options.Destination.Directory); //we work with already injected assemblies
            foreach (var dir in dirs)
            {
                runCtx.CurrentSourceDirectory = dir;
                await ProcessDirectory(runCtx).ConfigureAwait(false);
            }

            //possible files in the root directly
            if (!runCtx.Tree.GetAllAssemblies().Any()) //get only the needed assemblies
            {
                runCtx.CurrentSourceDirectory = runCtx.RootDirectory;
                await ProcessDirectory(runCtx).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Process directory from current Engine's context
        /// </summary>
        /// <param name="runCtx">Context of Engine's Run</param>
        /// <returns>Is the directory processed?</returns>
        internal async Task<bool> ProcessDirectory(RunContext runCtx)
        {
            var directory = runCtx.CurrentSourceDirectory;
            if (!InjectorCoreUtils.IsNeedProcessDirectory(Options.Filter, directory, directory == runCtx.Options.Destination.Directory))
                return false;
            _logger.Info($"Processing dir [{directory}]");

            //files
            var files = GetAssemblies(directory);
            foreach (var file in files)
            {
                runCtx.CurrentSourceFile = file;
                await ProcessFile(runCtx).ConfigureAwait(false);
            }

            //subdirectories
            var dirs = Directory.GetDirectories(directory, "*");
            foreach (var dir in dirs)
            {
                runCtx.CurrentSourceDirectory = dir;
                await ProcessDirectory(runCtx).ConfigureAwait(false);
            }
            return true;
        }

        private async Task<bool> ProcessFile(RunContext runCtx)
        {
            #region Checks
            //filter
            var filePath = runCtx.CurrentSourceFile;
            if (!InjectorCoreUtils.IsNeedProcessFile(Options.Filter, filePath))
                return false;
            #endregion

            try
            {
                //reading
                var reader = new AssemblyReader();
                using var asmCtx = reader.ReadAssembly(runCtx);

                if (!Directory.Exists(asmCtx.DestinationDir))
                    Directory.CreateDirectory(asmCtx.DestinationDir);

                //processing
                var proxyNs = "";
                var key = asmCtx.DestinationKey;
                if (runCtx.ProxyNamespaceByKeys.ContainsKey(key))
                    proxyNs = runCtx.ProxyNamespaceByKeys[key];
                else
                    proxyNs = ProxyHelper.CreateProxyNamespace();

                InjectTo(asmCtx.Definition, proxyNs, asmCtx.Version.IsNetFramework);
                _logger.Debug($"Processed by plugin: [{filePath}]");

                //writing modified assembly and symbols to new file
                var writer = new AssemblyWriter();
                var tempFile = Path.GetTempFileName(); //we need use temp file
                var modifiedPath = writer.SaveAssembly(runCtx, asmCtx, tempFile);
                asmCtx.Dispose();
                File.Copy(tempFile, filePath, true);
                File.Delete(tempFile);
                _logger.Info($"Writed: [{filePath}]");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Processing of file by plugin failed: {runCtx.CurrentSourceFile}");
                if (runCtx.Options.Debug?.IgnoreErrors != true)
                    throw;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get .dll from the specified directory
        /// </summary>
        /// <param name="directory">Directory with Target assemblies</param>
        /// <returns></returns>
        public virtual IEnumerable<string> GetAssemblies(string directory)
        {
            return Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly)
                .Where(a => a.EndsWith(".dll"));
        }
        #endregion
        #region Injection
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
            InjectInitMethod(assembly, type, typeof(TechTalk.SpecFlow.BeforeTestRunAttribute), proxyNs, isNetFX);
            InjectTestsFinished(assembly, type, proxyNs, isNetFX);
            InjectContextDataInvoker(type, isNetFX);
            //
            //InjectHook(type, proxyNs, typeof(TechTalk.SpecFlow.BeforeFeatureAttribute), "FeatureContext", "FeatureInfo", "Drill4NetFeatureStarting", 0, isNetFX);
            //InjectHook(type, proxyNs, typeof(TechTalk.SpecFlow.AfterFeatureAttribute), "FeatureContext", "FeatureInfo", "Drill4NetFeatureFinished", 1, isNetFX);
            InjectHook(assembly, type, proxyNs, typeof(TechTalk.SpecFlow.BeforeScenarioAttribute), "Drill4NetScenarioStarting", (int)AgentCommandType.TEST_CASE_START, isNetFX);
            InjectHook(assembly, type, proxyNs, typeof(TechTalk.SpecFlow.AfterScenarioAttribute), "Drill4NetScenarioFinished", (int)AgentCommandType.TEST_CASE_STOP, isNetFX);
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

        private void InjectInitMethod(AssemblyDefinition assembly, TypeDefinition type, Type methAttrType, string proxyNs, bool isNetFX)
        {
            var module = type.Module;
            var syslib = GetSysModule(isNetFX); //inner caching & disposing

            //field _scenarioMethInfo
            var fld_ProfilerProxy_methInfo = new FieldDefinition(_scenarioField, FieldAttributes.Private | FieldAttributes.Static, ImportSysTypeReference(syslib, module, typeof(System.Reflection.MethodInfo)));
            type.Fields.Add(fld_ProfilerProxy_methInfo);

            //method
            const string funcName = "Drill4NetTestsInit";
            var funcDef = new MethodDefinition(funcName, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, module.TypeSystem.Void);
            type.Methods.Add(funcDef);

            AddMethodAttribute(module, methAttrType, funcDef, false);

            funcDef.Body.InitLocals = true;
            var il_meth = funcDef.Body.GetILProcessor();
            //
            //var profPath = @"d:\Projects\EPM-D4J\!!_exp\Injector.Net\Agent.Test\bin\Debug\netstandard2.0\Agent.Test.dll";
            var lv_profPath1 = new VariableDefinition(module.TypeSystem.String);
            funcDef.Body.Variables.Add(lv_profPath1);
            var Ldstr2 = il_meth.Create(OpCodes.Ldstr, Path.Combine(HelperReadDir, HelperAsmName));
            il_meth.Append(Ldstr2);
            var Stloc3 = il_meth.Create(OpCodes.Stloc, lv_profPath1);
            il_meth.Append(Stloc3);

            //var asm = Assembly.LoadFrom(profPath);
            var lv_asm4 = new VariableDefinition(ImportSysTypeReference(syslib, module, typeof(System.Reflection.Assembly)));
            funcDef.Body.Variables.Add(lv_asm4);

            var methLoadFrom = ImportSysMethodReference(syslib, module, "System.Reflection", "Assembly", "LoadFrom", true, typeof(string), typeof(System.Reflection.Assembly));
            //module.ImportReference(TypeHelpers.ResolveMethod(coreLib, "System.Reflection.Assembly", "LoadFrom", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, "", "System.String"));

            var Call5 = il_meth.Create(OpCodes.Call, methLoadFrom);
            var Ldloc6 = il_meth.Create(OpCodes.Ldloc, lv_profPath1);
            il_meth.Append(Ldloc6);
            il_meth.Append(Call5);
            var Stloc7 = il_meth.Create(OpCodes.Stloc, lv_asm4);
            il_meth.Append(Stloc7);

            //var type = asm.GetType("Agent.Tests.LoggerAgent");
            var lv_type8 = new VariableDefinition(ImportSysTypeReference(syslib, module, typeof(Type)));
            funcDef.Body.Variables.Add(lv_type8);
            var Ldloc9 = il_meth.Create(OpCodes.Ldloc, lv_asm4);
            il_meth.Append(Ldloc9);

            var methGetType = ImportSysMethodReference(syslib, module, "System.Reflection", "Assembly", "GetType", false, typeof(string), typeof(Type));
            //module.ImportReference(TypeHelpers.ResolveMethod(coreLib, "System.Reflection.Assembly", "GetType", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, "", "System.String"));

            var Callvirt10 = il_meth.Create(OpCodes.Callvirt, methGetType);
            var Ldstr11 = il_meth.Create(OpCodes.Ldstr, $"{HelperNs}.{HelperClass}");
            il_meth.Append(Ldstr11);
            il_meth.Append(Callvirt10);
            var Stloc12 = il_meth.Create(OpCodes.Stloc, lv_type8);
            il_meth.Append(Stloc12);

            #region _methRegInfo = type.GetMethod("GetScenarioContext");
            var Ldloc13 = il_meth.Create(OpCodes.Ldloc, lv_type8);
            il_meth.Append(Ldloc13);

            var methGetMethodRef = ImportSysMethodReference(syslib, module, "System", "Type", "GetMethod", false, typeof(string), typeof(System.Reflection.MethodInfo));
            //module.ImportReference(TypeHelpers.ResolveMethod(coreLib, "System.Type", "GetMethod", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, "", "System.String"));

            var Callvirt14 = il_meth.Create(OpCodes.Callvirt, methGetMethodRef);
            var Ldstr15 = il_meth.Create(OpCodes.Ldstr, "GetScenarioContext");
            il_meth.Append(Ldstr15);
            il_meth.Append(Callvirt14);
            var Stsfld16 = il_meth.Create(OpCodes.Stsfld, fld_ProfilerProxy_methInfo);
            il_meth.Append(Stsfld16);
            #endregion

            //Method : DoCommand for TESTS_START
            var m_DoCommand = GetDoCommandMethod(proxyNs, assembly, isNetFX);
            il_meth.Emit(OpCodes.Ldc_I4, (int)AgentCommandType.ASSEMBLY_TESTS_START);
            il_meth.Emit(OpCodes.Ldnull);
            il_meth.Emit(OpCodes.Call, m_DoCommand);

            var Ret17 = il_meth.Create(OpCodes.Ret);
            il_meth.Append(Ret17);
        }

        private void InjectTestsFinished(AssemblyDefinition assembly, TypeDefinition type, string proxyNs, bool isNetFX)
        {
            var module = type.Module;

            const string funcName = "Drill4NetTestsFinished";
            var funcDef = new MethodDefinition(funcName, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, module.TypeSystem.Void);
            type.Methods.Add(funcDef);

            AddMethodAttribute(module, typeof(TechTalk.SpecFlow.AfterTestRunAttribute), funcDef, false);

            funcDef.Body.InitLocals = true;
            var ilProc = funcDef.Body.GetILProcessor();

            var m_DoCommand = GetDoCommandMethod(proxyNs, assembly, isNetFX);
            ilProc.Emit(OpCodes.Ldc_I4, (int)AgentCommandType.ASSEMBLY_TESTS_STOP);
            ilProc.Emit(OpCodes.Ldnull);
            ilProc.Emit(OpCodes.Call, m_DoCommand);

            ilProc.Append(ilProc.Create(OpCodes.Ret));
        }

        private void InjectContextDataInvoker(TypeDefinition classType, bool isNetFX)
        {
            var module = classType.Module;
            var syslib = GetSysModule(isNetFX); //inner caching & disposing

            //Method : GetContextData
            var m_GetContextData_2 = new MethodDefinition(_getContextDataMethod, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, module.TypeSystem.Void);
            m_GetContextData_2.ReturnType = module.TypeSystem.String;
            classType.Methods.Add(m_GetContextData_2);
            m_GetContextData_2.Body.InitLocals = true;
            var ilProc = m_GetContextData_2.Body.GetILProcessor();

            //Parameters of 'public static string GetContextData(MethodInfo meth, object featureCtx, object scenarioCtx)'
            var p_meth_4 = new ParameterDefinition("meth", ParameterAttributes.None, module.ImportReference(typeof(System.Reflection.MethodInfo)));
            m_GetContextData_2.Parameters.Add(p_meth_4);
            var p_featureCtx_5 = new ParameterDefinition("featureCtx", ParameterAttributes.None, module.TypeSystem.Object);
            m_GetContextData_2.Parameters.Add(p_featureCtx_5);
            var p_scenarioCtx_6 = new ParameterDefinition("scenarioCtx", ParameterAttributes.None, module.TypeSystem.Object);
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
            ilProc.Emit(OpCodes.Newarr, module.TypeSystem.Object);
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

        private void InjectHook(AssemblyDefinition assembly, TypeDefinition type, string proxyNs, Type methAttrType, string funcName, int command, bool isNetFX)
        {
            var module = type.Module;
            var syslib = GetSysModule(isNetFX); //inner caching & disposing

            //var funcName = "Drill4NetScenarioStarting";
            var funcDef = new MethodDefinition(funcName, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, module.TypeSystem.Void);
            type.Methods.Add(funcDef);

            AddMethodAttribute(module, methAttrType, funcDef);

            funcDef.Body.InitLocals = true;
            var ilProc = funcDef.Body.GetILProcessor();

            //Parameters of 'public static void Drill4NetScenarioStarting(FeatureContext featureContext, ScenarioContext scenarioContext)'
            var methParamRef1 = module.ImportReference(new TypeReference(SPEC_NS, "FeatureContext", _speclib, _speclib));
            var par1 = new ParameterDefinition("featureCtx", ParameterAttributes.None, methParamRef1);
            funcDef.Parameters.Add(par1);

            var methParamRef2 = module.ImportReference(new TypeReference(SPEC_NS, "ScenarioContext", _speclib, _speclib));
            var par2 = new ParameterDefinition("scenarioCtx", ParameterAttributes.None, methParamRef2);
            funcDef.Parameters.Add(par2);

            //var data = GetContextData(_scenarioMethInfo, featureContext, scenarioContext);
            var lv_data_6 = new VariableDefinition(module.TypeSystem.String);
            funcDef.Body.Variables.Add(lv_data_6);

            //Method : GetContextData
            var m_GetContextData_7 = type.Methods.Single(a => a.Name == _getContextDataMethod);
            m_GetContextData_7.ReturnType = module.TypeSystem.String;
            var fld_scenarioMethInfo_1 = type.Fields.Single(a => a.Name == _scenarioField);
            ilProc.Emit(OpCodes.Ldsfld, fld_scenarioMethInfo_1);
            ilProc.Emit(OpCodes.Ldarg_0);
            ilProc.Emit(OpCodes.Ldarg_1);
            ilProc.Emit(OpCodes.Call, m_GetContextData_7);
            ilProc.Emit(OpCodes.Stloc, lv_data_6);

            //Method : DoCommand
            var m_DoCommand = GetDoCommandMethod(proxyNs, assembly, isNetFX);
            //m_DoCommand.ReturnType = module.TypeSystem.Void;
            ilProc.Emit(OpCodes.Ldc_I4, command);
            ilProc.Emit(OpCodes.Ldloc, lv_data_6);
            ilProc.Emit(OpCodes.Call, m_DoCommand);

            ilProc.Append(ilProc.Create(OpCodes.Ret));
        }

        /// <summary>
        /// Get the DoCommand method of the Proxy class
        /// </summary>
        /// <param name="proxyNs"></param>
        /// <param name="assembly"></param>
        /// <param name="isNetFX"></param>
        /// <returns></returns>
        internal MethodDefinition GetDoCommandMethod(string proxyNs, AssemblyDefinition assembly, bool isNetFX)
        {
            var types = assembly.MainModule.Types;
            var proxyDef = types.SingleOrDefault(a => a.Namespace == proxyNs);
            if (proxyDef == null)
            {
                _proxyGenerator.InjectTo(assembly, proxyNs, isNetFX);
                proxyDef = types.SingleOrDefault(a => a.Namespace == proxyNs);
            }
            var m_DoCommand_8 = proxyDef.Methods.Single(a => a.Name == METHOD_COMMAND_NAME);
            return m_DoCommand_8;
        }

        internal void AddMethodAttribute(ModuleDefinition module, Type attribType,
                ICustomAttributeProvider targetMember, // Can be a PropertyDefinition, MethodDefinition or other member definitions
                bool needZeroOrder = true)
        {
            // ctor
            var constructor = attribType.GetConstructors()[0];
            var constructorRef = module.ImportReference(constructor);
            var attrib = new CustomAttribute(constructorRef);

            // The argument
            if (needZeroOrder)
            {
                var arg = new CustomAttributeArgument(module.ImportReference(typeof(int)), 0);
                attrib.ConstructorArguments.Add(arg);
                attrib.Properties.Add(new CustomAttributeNamedArgument("Order", arg));
            }
            targetMember.CustomAttributes.Add(attrib);
        }
        #endregion

        //TODO: full pattern!
        public override void Dispose()
        {
            _speclib.Dispose();
            _speclib = null;
            base.Dispose();
        }
    }
}
