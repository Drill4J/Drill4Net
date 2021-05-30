using System;
using System.Linq;
using System.Diagnostics;
using Serilog;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Drill4Net.Common;
using Drill4Net.Injection;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Engine
{
    /// <summary>
    /// The concrete Injector which injects the instrumenting code called by Target for Agent
    /// </summary>
    public class AssemblyInjector : IAssemblyInjector
    {
        /// <summary>
        /// Concrete strategy of instrumenting code injections into Target
        /// </summary>
        public CodeHandlerStrategy Strategy { get; }

        /**********************************************************************************/

        /// <summary>
        /// Create the Injector which injects the instrumenting code called by Target for Agent
        /// </summary>
        /// <param name="strategy"></param>
        public AssemblyInjector(CodeHandlerStrategy strategy)
        {
            Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        /**********************************************************************************/

        /// <summary>
        /// Inject the specified assembly
        /// </summary>
        /// <param name="runCtx">Context of Engine's Run</param>
        /// <param name="asmCtx">Context of current assembly</param>
        public void Inject(RunContext runCtx, AssemblyContext asmCtx)
        {
            if (!ContextHelper.CreateContexts(runCtx, asmCtx))
                return; //it's normal (in the most case it's means the assembly is shared and already is injected)

            //preparing
            ContextHelper.PrepareContextData(runCtx, asmCtx);
            AssemblyHelper.FindMoveNextMethods(asmCtx);
            AssemblyHelper.MapBusinessMethodFirstPass(asmCtx);
            AssemblyHelper.MapBusinessMethodSecondPass(asmCtx);
            AssemblyHelper.CalcBusinessPartCodeSizes(asmCtx);

            //get the injecting commands
            var opts = runCtx.Options;
            asmCtx.ProxyNamespace = ProxyHelper.CreateProxyNamespace();
            asmCtx.ProxyMethRef = ProxyHelper.CreateProxyMethodReference(asmCtx, opts);

            //the injecting is here
            InjectProxyCalls(asmCtx, runCtx.Tree);
            InjectProxyType(asmCtx, opts);

            //coverage data
            CoverageHelper.CalcCoverageBlocks(asmCtx);
        }

        /// <summary>
        /// Inject instrumenting probe calls of Proxy class' calls 
        /// for each needed cross-point of Target
        /// </summary>
        /// <param name="asmCtx"></param>
        /// <param name="tree"></param>
        internal void InjectProxyCalls(AssemblyContext asmCtx, InjectedSolution tree)
        {
            foreach (var typeCtx in asmCtx.TypeContexts.Values)
            {
                Debug.WriteLine(typeCtx.InjType.FullName);

                //process methods
                foreach (var methodCtx in typeCtx.MethodContexts.Values)
                {
                    Debug.WriteLine(methodCtx.Method.FullName);

                    var methodDef = methodCtx.Definition;
                    var body = methodDef.Body;
                    //body.SimplifyMacros(); //bug (Cecil's or my?)
                    var instructions = methodCtx.Instructions; //no copy list!

                    #region Jumpers
                    //collect jumpers. Hash table for separate addresses is almost useless,
                    //because they may be recalculated inside the processor during inject...
                    //and ideally, there shouldn't be too many of them 
                    for (var i = 1; i < instructions.Count; i++)
                    {
                        var instr = instructions[i];
                        var flow = instr.OpCode.FlowControl;
                        if (flow is not (FlowControl.Branch or FlowControl.Cond_Branch))
                            continue;
                        methodCtx.Jumpers.Add(instr);
                        //
                        var anchor = instr.Operand;
                        //need this jump for handle?
                        var curCode = instr.OpCode.Code;
                        //not needed jumps from by Leave from try/catch/finally semantically
                        if (curCode == Code.Leave || curCode == Code.Leave_S)
                            continue;
                        if (instr.Next != anchor && !methodCtx.Anchors.Contains(anchor))
                            methodCtx.Anchors.Add(anchor);
                    }
                    #endregion
                    #region CG method's global call index
                    //these methods are only of the current assembly, but this is enough to work with CG methods
                    //This should be done here, for an already gathered dependency tree
                    var treeMethods = tree.Filter(typeof(InjectedMethod), true)
                        .Cast<InjectedMethod>()
                        .Where(a => a.CalleeIndexes.Count > 0);
                    foreach (var caller in treeMethods)
                    {
                        foreach (var calleName in caller.CalleeIndexes.Keys)
                        {
                            if (asmCtx.InjMethodByFullname.ContainsKey(calleName))
                            {
                                var callee = asmCtx.InjMethodByFullname[calleName];
                                var cgInfo = callee.CGInfo;
                                if (cgInfo == null) //null is normal (business method)
                                    continue;
                                cgInfo.Caller = caller;
                                cgInfo.CallerIndex = caller.CalleeIndexes[calleName];
                            }
                            else { } //hmmm... check, WTF...
                        }
                    }
                    #endregion
                    #region *** Injections ***
                    Strategy.Preprocess(methodCtx); //primary actions
                    for (var i = methodCtx.StartIndex; i < instructions.Count; i++)
                    {
                        #region Checks
                        var instr = instructions[i];
                        if (!methodCtx.BusinessInstructions.Contains(instr))
                            continue;
                        if (methodCtx.AheadProcessed.Contains(instr))
                            continue;
                        #endregion

                        methodCtx.SetPosition(i);
                        i = HandleInstruction(methodCtx); //process and correct current index after potential injection
                    }
                    #endregion
                    #region Correct jumps
                    //EACH short form -> to long form (otherwise, we need to recalculate 
                    //again after each necessary conversion)
                    var jumpers = methodCtx.Jumpers.ToArray();
                    foreach (var jump in jumpers)
                    {
                        var opCode = jump.OpCode;
                        if (jump.Operand is not Instruction)
                            continue;
                        var newOpCode = InstructionHelper.ShortJumpToLong(opCode);
                        if (newOpCode.Code != opCode.Code)
                            jump.OpCode = newOpCode;
                    }
                    #endregion

                    body.Optimize();
                    body.OptimizeMacros();
                }
            }
        }

        /// <summary>
        /// Process the current instruction of IL code according to <see cref="Strategy"/>
        /// </summary>
        /// <param name="ctx">Current method context</param>
        /// <returns></returns>
        internal int HandleInstruction(MethodContext ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));
            try
            {
                Strategy.HandleInstruction(ctx);
                return ctx.CurIndex;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Handling instruction: {ctx.ModuleName}; {ctx.Method.FullName}; {nameof(ctx.CurIndex)}: {ctx.CurIndex}");
                throw;
            }
        }

        /// <summary>
        /// Inject into assembly the Proxy class (it just pushes the probe data from 
        /// Target class to real Agent) according specified in context and options metadata
        /// </summary>
        /// <param name="asmCtx">Assembly context</param>
        /// <param name="opts">Injector options</param>
        internal void InjectProxyType(AssemblyContext asmCtx, InjectorOptions opts)
        {
            //here we generate proxy class which will be calling of real profiler by cached Reflection
            //directory of profiler dependencies - for injected target on it's side
            var module = asmCtx.Module;
            var assembly = asmCtx.Definition;
            var profilerOpts = opts.Profiler;
            var profDir = profilerOpts.Directory;
            var proxyGenerator = new ProfilerProxyGenerator(asmCtx.ProxyNamespace, opts.Proxy.Class, opts.Proxy.Method, //proxy to profiler
                                                            profDir, profilerOpts.AssemblyName, //real profiler
                                                            profilerOpts.Namespace, profilerOpts.Class, profilerOpts.Method);
            var isNetFx = asmCtx.Version.Target == AssemblyVersionType.NetFramework;
            proxyGenerator.InjectTo(assembly, isNetFx);

            // ensure we referencing only ref assemblies
            if (isNetFx)
            {
                var systemPrivateCoreLib = module.AssemblyReferences
                    .FirstOrDefault(x => x.Name.StartsWith("System.Private.CoreLib", StringComparison.InvariantCultureIgnoreCase));
                //Debug.Assert(systemPrivateCoreLib == null, "systemPrivateCoreLib == null");
                if (systemPrivateCoreLib != null)
                    module.AssemblyReferences.Remove(systemPrivateCoreLib);
            }
        }
    }
}
