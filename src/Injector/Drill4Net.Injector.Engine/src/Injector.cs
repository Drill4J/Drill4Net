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
    public class Injector : IInjector
    {
        public CodeHandlerStrategy Strategy { get; }

        /**********************************************************************************/

        public Injector(CodeHandlerStrategy strategy)
        {
            Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        /**********************************************************************************/

        public void Inject(RunContext runCtx, AssemblyContext asmCtx)
        {
            if (!ContextHelper.CreateContexts(runCtx, asmCtx))
                return; //it's normal (in the most case it's means the assembly is shared and already is injected)

            //get the injecting commands
            var opts = runCtx.Options;
            asmCtx.ProxyNamespace = ProxyHelper.CreateProxyNamespace();
            asmCtx.ProxyMethRef = ProxyHelper.CreateProxyMethodReference(asmCtx, opts);

            //preparing
            ContextHelper.PrepareContextData(runCtx, asmCtx);

            AssemblyHelper.FindMoveNextMethods(asmCtx);
            AssemblyHelper.MapBusinessMethodFirstPass(asmCtx);
            AssemblyHelper.MapBusinessMethodSecondPass(asmCtx);
            AssemblyHelper.CalcBusinessPartCodeSizes(asmCtx);

            //the injecting here
            InjectProxyCalls(asmCtx, runCtx.Tree);
            InjectProxyClass(asmCtx, opts);

            //coverage data
            CoverageHelper.CalcCoverageBlocks(asmCtx);
        }

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
                    //body.SimplifyMacros(); //buggy (Cecil or me?)
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
                    Strategy.StartMethod(methodCtx); //primary actions
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

        internal void InjectProxyClass(AssemblyContext asmCtx, InjectorOptions opts)
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
