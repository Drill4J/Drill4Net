using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Serilog;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Drill4Net.Common;
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
        /// <param name="runCtx">Context of Injector Engine's Run</param>
        /// <param name="asmCtx">Context of current assembly</param>
        public void Inject(RunContext runCtx, AssemblyContext asmCtx)
        {
            if (!AssemblyHelper.PrepareInjectedAssembly(runCtx, asmCtx))
                return; //it's normal (in the most case it's means the assembly is shared and already is injected)

            if (!ContextHelper.PrepareContextData(runCtx, asmCtx))
                return; //just the context does not contain any methods of interest to us

            //the preparing
            AssemblyHelper.FindMoveNextMethods(asmCtx);
            AssemblyHelper.MapBusinessMethodFirstPass(asmCtx);
            AssemblyHelper.MapBusinessMethodSecondPass(asmCtx);
            AssemblyHelper.CalcBusinessPartCodeSizes(asmCtx);

            //the injecting
            InjectProxyCalls(asmCtx, runCtx.Tree);
            InjectProxyType(runCtx, asmCtx);

            //prepare coverage metadata
            CoverageHelper.CalcCoverageBlocks(asmCtx);
        }

        /// <summary>
        /// Inject instrumenting probe calls of Proxy class' calls 
        /// for each needed cross-point of Target
        /// </summary>
        /// <param name="asmCtx">Assembly's context</param>
        /// <param name="tree">The tree of the injected entities</param>
        internal void InjectProxyCalls(AssemblyContext asmCtx, InjectedSolution tree)
        {
            //these methods are only of the current assembly, but this is enough to work with CG methods
            //This should be done here, for an already gathered dependency tree
            var treeAsmMethods = tree.Filter(typeof(InjectedMethod), true)
                .Cast<InjectedMethod>()
                .Where(a => a.CalleeIndexes.Count > 0)
                .Where(s => s.AssemblyName == asmCtx.InjAssembly.Name);

            //by types
            foreach (var typeCtx in asmCtx.TypeContexts.Values) 
            {
                Debug.WriteLine(typeCtx.InjType.FullName);

                //by methods
                foreach (var methodCtx in typeCtx.MethodContexts.Values) 
                {
                    Debug.WriteLine(methodCtx.Method.FullName);

                    var body = methodCtx.Definition.Body;
                    //body.SimplifyMacros(); //bug (Cecil's or my?)

                    CollectJumpers(methodCtx);
                    CollectCallsInfo(asmCtx, treeAsmMethods);
                    InjectMethod(methodCtx);
                    CorrectJumps(methodCtx.Jumpers.ToArray());

                    body.Optimize();
                    body.OptimizeMacros();
                }
            }
        }

        internal void CollectCallsInfo(AssemblyContext asmCtx, IEnumerable<InjectedMethod> treeAsmMethods)
        {
            foreach (var caller in treeAsmMethods)
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
        }

        internal void InjectMethod(MethodContext methodCtx)
        {
            var instructions = methodCtx.Instructions; //no copy list!
            Strategy.Preprocess(methodCtx); //primary pre-actions for some handlers
            for (var i = methodCtx.StartIndex; i < instructions.Count; i++)
            {
                #region Checks
                var instr = instructions[i];
                if (!methodCtx.BusinessInstructions.Contains(instr))
                    continue;
                if (methodCtx.Processed.Contains(instr))
                    continue;
                #endregion

                methodCtx.SetPosition(i);
                i = HandleInstruction(methodCtx); //process and correct current index after potential injection
            }
            Strategy.Postprocess(methodCtx); //primary post-actions for some handlers
        }

        internal void CollectJumpers(MethodContext methodCtx)
        {
            //Hash table for separate addresses is almost useless,
            //because they may be recalculated inside the processor during inject...
            //and ideally, there shouldn't be too many of them 
            var instructions = methodCtx.Instructions; //no copy list!
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
        }

        /// <summary>
        ///EACH short form of jumps to long form (otherwise, we need to recalculate 
        ///again after each necessary conversion)
        /// </summary>
        /// <param name="jumpers"></param>
        internal void CorrectJumps(IEnumerable<Instruction> jumpers)
        {
            foreach (var jump in jumpers)
            {
                var opCode = jump.OpCode;
                if (jump.Operand is not Instruction)
                    continue;
                var newOpCode = InstructionHelper.ShortJumpToLong(opCode);
                if (newOpCode.Code != opCode.Code)
                    jump.OpCode = newOpCode;
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
        /// <param name="runCtx">The Injector Engine's Run</param>
        /// <param name="asmCtx">Assembly context</param>
        /// <param name="opts">Injector options</param>
        internal void InjectProxyType(RunContext runCtx, AssemblyContext asmCtx)
        {
            //here we generate proxy class which will be calling of real profiler by cached Reflection
            //directory of profiler dependencies - for injected target on it's side
            var isNetFx = asmCtx.Version.Target == AssemblyVersionType.NetFramework;
            runCtx.ProxyGenerator.InjectTo(asmCtx.Definition, asmCtx.ProxyNamespace, isNetFx);

            // ensure we referencing only ref assemblies
            if (isNetFx)
            {
                var module = asmCtx.Module;
                var systemPrivateCoreLib = module.AssemblyReferences
                    .FirstOrDefault(x => x.Name.StartsWith("System.Private.CoreLib", StringComparison.InvariantCultureIgnoreCase));
                if(systemPrivateCoreLib != null)
                    module.AssemblyReferences.Remove(systemPrivateCoreLib);
            }
        }
    }
}
