﻿using System.Linq;
using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class ReturnHandler : AbstractInstructionHandler
    {
        protected string _initProbData;
        protected Instruction _returnInst;

        /**************************************************************************************/

        public ReturnHandler() : base(InjectorCoreConstants.INSTRUCTION_HANDLER_RETURN)
        {          
        }

        /**************************************************************************************/

        protected override void StartMethodConcrete(InjectorContext ctx)
        {
            //data
            _initProbData = _probeHelper.GetProbeData(ctx.TreeMethod, ctx.ModuleName, CrossPointType.Return, -1);
            _returnInst = GetFirstInstruction(_initProbData); //as object it must be only one
        }

        protected override void HandleInstructionConcrete(InjectorContext ctx, out bool needBreak)
        {
            #region Init
            needBreak = false;
            var treeFunc = ctx.TreeMethod;
            var moduleName = ctx.ModuleName;

            var processor = ctx.Processor;
            var instructions = ctx.Instructions;
            var instr = instructions[ctx.CurIndex];
            var opCode = instr.OpCode;
            var code = opCode.Code;

            var exceptionHandlers = ctx.ExceptionHandlers;
            var jumpers = ctx.Jumpers;

            var strictEnterReturn = ctx.IsStrictEnterReturn;
            var call = Instruction.Create(OpCodes.Call, ctx.ProxyMethRef);
            var lastOp = ctx.LastOperation;
            #endregion

            if (strictEnterReturn)
                return;

            //the return in the middle of the method body
            if (instr.Operand == lastOp && !strictEnterReturn && lastOp.OpCode.Code != Code.Endfinally) //jump to the end for return from function
            {
                _returnInst.Operand = $"{_initProbData}{ctx.CurIndex}";
                instr.Operand = _returnInst;
            }

            //RETURN
            if (code != Code.Ret || strictEnterReturn) 
                return;

            _returnInst.Operand = $"{_initProbData}{ctx.CurIndex}";
            FixFinallyEnd(instr, _returnInst, exceptionHandlers);
            ReplaceJump(instr, _returnInst, jumpers);
            processor.InsertBefore(instr, _returnInst);
            processor.InsertBefore(instr, call);
            ctx.IncrementIndex(2);
                
            //correcting pointId
            var point = ctx.TreeMethod.Filter(typeof(CrossPoint), false)
                .Cast<CrossPoint>()
                .FirstOrDefault(a => a.PointType == CrossPointType.Return && a.PointId == null);
            if(point != null)
                point.PointId = ctx.CurIndex.ToString();
        }
    }
}
