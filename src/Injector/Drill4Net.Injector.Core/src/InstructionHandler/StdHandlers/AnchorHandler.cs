using Mono.Cecil.Cil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class AnchorHandler : AbstractSimpleHandler
    {
        private readonly bool _ignoreCycles;
        
        /************************************************************************************/
        
        public AnchorHandler(AbstractProbeHelper probeHelper, bool ignoreCycles) : 
            base(InjectorCoreConstants.INSTRUCTION_HANDLER_ANCHOR, CrossPointType.Anchor, probeHelper)
        {
            _ignoreCycles = ignoreCycles;
        }

        /************************************************************************************/

        protected override bool IsCondition(MethodContext ctx)
        {
            var instr = ctx.Instructions[ctx.CurIndex];
            var flow = instr.OpCode.FlowControl;
            //
            if (flow is not FlowControl.Next and not FlowControl.Call)
                return false;
            if (instr.Previous is {OpCode: {Code: Code.Leave or Code.Leave_S}}) 
                return false;
            if (ctx.CompilerInstructions.Contains(instr))
                return false;
            var desc = instr.Operand?.ToString();
            if (desc != null)
            {
                if (desc.Contains("get_IsCompleted()"))
                    return true;
            }
            if (ctx.Processed.Contains(instr)) //may be 'IF' or 'ELS' it yet are processed
                return false;
            if (!ctx.Anchors.Contains(instr))
                return false;
            
            // //we need use only 'pure jumps', for example, 'goto' statement,
            // //not if/else or cycles jumps, there are special handlers for these purposes
            // if (_ignoreCycles)
            // {
            //     foreach (var cur in ctx.Jumpers)
            //     {
            //         Instruction[] operands;
            //         if (cur.Operand is Instruction instruction)
            //             operands = new[] {instruction};
            //         else
            //             operands = (Instruction[])cur.Operand; //'switch' statement
            //         //
            //         foreach (var operand in operands)
            //         {
            //             var anchor = operand;
            //             if (ctx.FirstInjectInstructions.Contains(anchor) && ctx.ReplacedJumps.ContainsKey(anchor))
            //                 anchor = ctx.ReplacedJumps[anchor];
            //             if (anchor != instr)
            //                 continue;
            //             var jumpFlow = cur.OpCode.FlowControl;
            //             if (jumpFlow == FlowControl.Cond_Branch)
            //                 return false;
            //             if (jumpFlow == FlowControl.Branch && cur.Previous is {OpCode: {FlowControl: FlowControl.Cond_Branch}})
            //                 return false;
            //         }
            //     }
            // }
            //
            return true; 
        }
    }
}
