using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Engine
{
    /// <summary>
    /// Helper for getting some info about assemblies
    /// </summary>
    internal static class AssemblyHelper
    {
        /// <summary>
        /// Create and prepare the <see cref="InjectedAssembly"/> and its <see cref="InjectedDirectory"/> if needed
        /// </summary>
        /// <param name="runCtx">Context of the Injector Engine's Run</param>
        /// <param name="asmCtx">Assembly's context</param>
        /// <returns></returns>
        internal static bool PrepareInjectedAssembly(RunContext runCtx, AssemblyContext asmCtx)
        {
            var sourceDir = asmCtx.SourceDir;
            var destDir = asmCtx.DestinationDir;
            var asmFullName = asmCtx.Definition.FullName;
            var tree = runCtx.Tree;

            //directory
            var treeDir = tree.GetDirectory(sourceDir);
            if (treeDir == null)
            {
                treeDir = new InjectedDirectory(sourceDir, destDir);
                tree.Add(treeDir);
            }

            //assembly (exactly from whole tree, not just current treeDir - for shared dll)
            var asmPath = runCtx.SourceFile;
            var treeAsm = tree.GetAssembly(asmPath, true) ??
                          new InjectedAssembly(asmCtx.Version, asmCtx.Module.Name, asmFullName, asmPath);
            treeDir.Add(treeAsm);
            asmCtx.InjAssembly = treeAsm;

            var key = asmCtx.Key;
            var keys = runCtx.AssemblyPaths;
            if (keys.ContainsKey(key)) //the assembly is shared and already is injected
            {
                var writer = new AssemblyWriter();
                var copyFrom = keys[key];
                var copyTo = writer.GetDestFileName(copyFrom, destDir);
                File.Copy(copyFrom, copyTo, true);
                return false;
            }
            return true;
        }

        internal static void FindMoveNextMethods(AssemblyContext asmCtx)
        {
            var moveNextMethods = asmCtx.InjAssembly.Filter(typeof(InjectedMethod), true)
               .Cast<InjectedMethod>()
               .Where(x => x.IsCompilerGenerated && x.Name == "MoveNext")
               .ToList();
            for (int i = 0; i < moveNextMethods.Count; i++)
            {
                InjectedMethod meth = moveNextMethods[i];
                // Owner type
                var fullName = meth.FullName;
                var mkey = fullName.Split(' ')[1].Split(':')[0];
                if (asmCtx.InjClasses.ContainsKey(mkey))
                {
                    var treeType = asmCtx.InjClasses[mkey];
                    treeType.Add(meth);
                }
                // Business method
                var extRealMethodName = MethodHelper.TryGetBusinessMethod(meth.FullName, meth.Name, true, true);
                mkey = MethodHelper.GetMethodKey(meth.TypeName, extRealMethodName);
                if (!asmCtx.InjMethodByKeys.ContainsKey(mkey))
                    continue;
                var treeFunc = asmCtx.InjMethodByKeys[mkey];
                if (meth.CGInfo != null)
                    meth.CGInfo.FromMethod = treeFunc.FullName;
            }
        }

        internal static void MapBusinessMethodFirstPass(AssemblyContext asmCtx)
        {
            foreach (var typeCtx in asmCtx.TypeContexts.Values)
            {
                foreach (var methodCtx in typeCtx.MethodContexts.Values)
                {
                    #region Init
                    var startInd = methodCtx.StartIndex;
                    var instructions = methodCtx.Instructions;
                    var injMethod = methodCtx.Method;

                    var cgInfo = injMethod.CGInfo;
                    if (cgInfo != null)
                        cgInfo.FirstIndex = startInd == 0 ? 0 : startInd - 1; //correcting to real start
                    #endregion

                    for (var i = startInd; i < instructions.Count; i++)
                    {
                        methodCtx.SetPosition(i);
                        MethodHelper.MapBusinessFunction(methodCtx); //in any case check each instruction for mapping

                        #region Check
                        var checkRes = CheckInstruction(methodCtx);
                        if (checkRes == OperationType.CycleContinue)
                            continue;
                        if (checkRes == OperationType.BreakCycle)
                            break;
                        if (checkRes == OperationType.Return)
                            return;
                        #endregion

                        i = methodCtx.CurIndex; //because it can change
                        methodCtx.BusinessInstructions.Add(instructions[i]);
                    }
                    //
                    var cnt = methodCtx.BusinessInstructions.Count;
                    injMethod.BusinessSize = cnt;
                    injMethod.OwnBusinessSize = cnt;
                }
            }
        }

        internal static OperationType CheckInstruction(MethodContext ctx)
        {
            var instr = ctx.CurInstruction;
            var code = instr.OpCode.Code;

            // for injecting cases
            if (code == Code.Nop || ctx.AheadProcessed.Contains(instr))
                return OperationType.CycleContinue;

            var method = ctx.Method;

            #region Awaiters in MoveNext as a boundary
            //find the boundary between the business code and the compiler-generated one
            if (method.Source.IsMoveNext && method.Source.MethodType == MethodType.CompilerGenerated)
            {
                if (IsAsyncCompletedOperand(instr))
                {
                    //leave the current first try/catch
                    var res = LeaveTryCatch(ctx);

                    //is second 'get_IsCompleted' exists(for example, for 'async stream' method)?
                    if (res)
                    {
                        var instructions = ctx.Instructions;
                        for (var i = ctx.CurIndex + 1; i < instructions.Count; i++)
                        {
                            if (!IsAsyncCompletedOperand(instructions[i]))
                                continue;
                            ctx.SetPosition(i);
                            LeaveTryCatch(ctx);
                        }
                    }
                }
                else
                {
                    //+margin if instruction starts the try/catch
                    var delta = ctx.ExceptionHandlers.Any(a => a.TryStart == instr) ? 6 : 0;
                    ctx.CorrectIndex(delta);
                }
            }
            #endregion

            return OperationType.NextOperand;

            //local funcs
            static bool LeaveTryCatch(MethodContext ctx)
            {
                var instr = ctx.CurInstruction;
                var curTryCactches = ctx.ExceptionHandlers.Where(a => a.TryStart.Offset < instr.Offset && instr.Offset < a.HandlerEnd.Offset);
                if (curTryCactches.Any())
                {
                    var maxStart = curTryCactches.Max(a => a.TryStart.Offset);
                    var curTryCactch = curTryCactches.First(b => b.TryStart.Offset == maxStart);
                    var endInd = ctx.Instructions.IndexOf(curTryCactch.HandlerEnd) + 1;
                    ctx.SetPosition(endInd);
                    return true;
                }
                return false;
            }

            static bool IsAsyncCompletedOperand(Instruction instr)
            {
                return instr.Operand?.ToString().Contains("::get_IsCompleted()") == true;
            }
        }

        internal static void MapBusinessMethodSecondPass(AssemblyContext asmCtx)
        {
            var badCtxs = new List<MethodContext>();
            foreach (var typeCtx in asmCtx.TypeContexts.Values)
            {
                foreach (var methodCtx in typeCtx.MethodContexts.Values)
                {
                    var meth = methodCtx.Method;
                    if (meth.IsCompilerGenerated)
                    {
                        var methFullName = meth.FullName;
                        if (meth.BusinessMethod == methFullName)
                        {
                            var bizFunc = typeCtx.MethodContexts.Values.Select(a => a.Method)
                                .FirstOrDefault(a => a.CalleeIndexes.ContainsKey(methFullName));
                            if (bizFunc != null)
                                meth.CGInfo.Caller = bizFunc;
                            else
                                badCtxs.Add(methodCtx);
                        }
                    }
                }
            }

            // the removing methods for which business method is not defined 
            foreach (var ctx in badCtxs)
                ctx.TypeCtx.MethodContexts.Remove(ctx.Method.FullName);
        }

        internal static void CalcBusinessPartCodeSizes(AssemblyContext asmCtx)
        {
            var bizMethods = asmCtx.InjMethodByFullname.Values
                .Where(a => !a.IsCompilerGenerated).ToArray();
            if (!bizMethods.Any())
                return;
            foreach (var caller in bizMethods.Where(a => a.CalleeIndexes.Count > 0))
            {
                foreach (var calleeName in caller.CalleeIndexes.Keys)
                    MethodHelper.CorrectMethodBusinessSize(asmCtx.InjMethodByFullname, caller, calleeName);
            }
        }
    }
}
