﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        internal async static Task<bool> PrepareInjectedAssembly(RunContext runCtx, AssemblyContext asmCtx)
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
                //TODO: we need copy existing PDB files too (when they will be changed according to the new injected reality)!
                try
                {
                    File.Copy(copyFrom, copyTo, true);
                }
                catch
                {
                    await Task.Delay(1000).ConfigureAwait(false);
                    File.Copy(copyFrom, copyTo, true);
                }
                return false;
            }
            return true;
        }

        internal static void CalcMethodHashcodes(AssemblyContext asmCtx)
        {
            var methCtxs = asmCtx.TypeContexts.Values.SelectMany(a => a.MethodContexts.Values).ToDictionary(a => a.Method.FullName);
            var bizMethCtxs = methCtxs.Values.Where(a => !a.Method.IsCompilerGenerated).ToDictionary(a => a.Method.FullName);
            foreach (var fullname in bizMethCtxs.Keys)
            {
                var methCtx = methCtxs[fullname];
                var code = MethodHelper.GetMethodHashCode(methCtx.Definition.Body.Instructions);

                //check the CG callees
                CalcWithCalleeHashCodes(ref code, methCtx, methCtxs);

                methCtx.Method.Source.HashCode = code.ToString();
            }
        }

        internal static void CalcWithCalleeHashCodes(ref int currentCode, MethodContext methCtx, Dictionary<string, MethodContext> methCtxs)
        {
            var callees = methCtx.Method.CalleeOrigIndexes;
            foreach (var calleeName in callees.Keys)
            {
                if (!methCtxs.TryGetValue(calleeName, out MethodContext calleeCtx))
                    continue; //it's normal
                var callee = calleeCtx.Method;
                if (!callee.IsCompilerGenerated) //we need ONLY CG chain of the root business method
                    continue;
                var calleeCode = MethodHelper.GetMethodHashCode(calleeCtx.Definition.Body.Instructions);
                currentCode ^= calleeCode;
                CalcWithCalleeHashCodes(ref currentCode, calleeCtx, methCtxs);
            }
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
                mkey = MethodHelper.GetMethodKey(meth.BusinessType, extRealMethodName);
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
                        var instr = instructions[i];
                        methodCtx.BusinessInstructions.Add(instr);
                        methodCtx.BusinessInstructionList.Add(instr);
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
            if (code == Code.Nop || ctx.Processed.Contains(instr))
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
                                .FirstOrDefault(a => a.CalleeOrigIndexes.ContainsKey(methFullName));
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
            foreach (var caller in bizMethods.Where(a => a.CalleeOrigIndexes.Count > 0))
            {
                foreach (var calleeName in caller.CalleeOrigIndexes.Keys)
                    MethodHelper.CorrectMethodBusinessSize(asmCtx.InjMethodByFullname, caller, calleeName);
            }
        }

        #region CalcBusinessIndex
        internal static void CorrectBusinessIndexes(AssemblyContext asmCtx)
        {
            foreach (var typeCtx in asmCtx.TypeContexts.Values)
            {
                var allMethCtxs = typeCtx.MethodContexts.Values;
                var bizMethCtxs = typeCtx.MethodContexts.Values.Where(a => !a.Method.IsCompilerGenerated);
                foreach (var methodCtx in bizMethCtxs)
                {
                    var delta = 0;
                    List<(int Index, string Uid)> end2EndBizIndexes = new();
                    CorrectBusinessIndexesForMethodCtx(allMethCtxs, methodCtx, ref delta, ref end2EndBizIndexes);
                    var method = methodCtx.Method;
                    method.End2EndBusinessIndexes = end2EndBizIndexes;

                    //checks
                    var inds = method.End2EndBusinessIndexes;
                    var cnt = inds.Count;
                    var retInd = inds[cnt - 1].Index;
                    if (method.BusinessSize != retInd + 1)
                    { 
                        //TODO: log
                        //throw new System.Exception($"Wrong business indexes for [{method}]");
                    }

                    // tests TODO: unit tests
                    if (asmCtx.Options.Probes.SkipIfElseType)
                    {
                        for (var i = 0; i < cnt - 1; i++)
                        {
                            if (inds[i].Index > inds[i + 1].Index) //equal can be (e.g. cycles)
                            { }
                        }
                    }
                }
            }
        }

        internal static void CorrectBusinessIndexesForMethodCtx(IEnumerable<MethodContext> methCtxs, MethodContext methodCtx,
            ref int delta, ref List<(int Index, string Uid)> end2EndBusinessIndexes)
        {
            var meth = methodCtx.Method;
            var points = meth.Points.ToList(); //it's new list, not original one

            //find orphan CG callees without real call instructions (method sigs) in current method 
            var callPoints = points.Where(a => a.PointType is CrossPointType.Call).ToDictionary(a => a.OrigInd);
            var usedIndexes = new HashSet<int>();
            foreach (var calleeName in meth.CalleeOrigIndexes.Keys)
            {
                var ind = meth.CalleeOrigIndexes[calleeName];
                var calleeCtx = methCtxs.FirstOrDefault(a => a.Method.FullName == calleeName);
                if (calleeCtx?.Method.IsCompilerGenerated != true) //not CG method
                {
                    //with one call for the callee can be linked many method sigs (e.g. in IL's ldftn instruction)
                    //e.g. for IHS project: Ipreo.Csp.IaDeal.Api.Bdd.Tests.Helpers -> StepsTransformation.ToDealVersionWithTranches ->
                    //ldftn with sig '...<ToDealVersionWithTranches>b__72_2 ...'
                    if (!usedIndexes.Contains(ind))
                        usedIndexes.Add(ind);
                    continue;
                }
                if (!usedIndexes.Contains(ind) && callPoints.ContainsKey(ind)) //first method sig linked to one call
                {
                    usedIndexes.Add(ind);
                    continue;
                }

                //second method sig - we need to link it to the new virtual point
                points.Add(new CrossPoint(calleeName, ind, ind, CrossPointType.Virtual)); //add auxiliary virtual cross-point
            }

            // calc delta for the biz index
            var orderedPoints = points.OrderBy(a => a.OrigInd);
            foreach (var point in orderedPoints) //by ordered points
            {
                var origInd = point.OrigInd;
                var localBizInd = methodCtx.GetLocalBusinessIndex(origInd); //only for the local code body
                var bizInd = localBizInd + delta; //biz index for the calling point itself DON'T include the body of its callee

                end2EndBusinessIndexes.Add((bizInd, point.PointUid));
                point.BusinessIndex = bizInd;

                if (point.PointType is CrossPointType.Call or CrossPointType.Virtual)
                {
                    var callee = point.PointType is CrossPointType.Virtual ?
                        point.PointUid :
                        meth.CalleeOrigIndexes.FirstOrDefault(a => a.Value == origInd).Key;
                    if (callee == null)
                    { } //bad... remove the point from data?
                    if (callee != null) //meth call the callee
                    {
                        var calleeCtx = methCtxs.FirstOrDefault(a => a.Method.FullName == callee);
                        if (calleeCtx?.Method.IsCompilerGenerated == true) //...and we need to include this callee to biz index of its caller
                        {
                            delta = bizInd; //shift for the callee taking into account the index of its call instruction in parent
                            //delta will be increased in the body of that CG method for NEXT instructions of the parent method
                            CorrectBusinessIndexesForMethodCtx(methCtxs, calleeCtx, ref delta, ref end2EndBusinessIndexes);
                            delta -= localBizInd; //correct for local using
                        }
                        if(calleeCtx == null)
                        { }
                    }
                }
            }
            //
           delta += methodCtx.BusinessInstructionList.Count - 1;
        }
        #endregion

        internal static MethodContext GetMethodContext(AssemblyContext asmCtx, string methodName)
        {
            if (!asmCtx.InjMethodByFullname.ContainsKey(methodName))
                return null;
            var callee = asmCtx.InjMethodByFullname[methodName];
            var calleeType = $"{callee.Signature.Namespace}.{callee.Signature.Type}";
            if (!asmCtx.TypeContexts.ContainsKey(calleeType))
                return null;
            var calleeTypeCtx = asmCtx.TypeContexts[calleeType];
            var calleeCtx = calleeTypeCtx.MethodContexts[methodName];
            return calleeCtx;
        }
    }
}
