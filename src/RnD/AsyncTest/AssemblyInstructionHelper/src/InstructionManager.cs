using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Drill4Net.AssemblyInstruction.Helper
{
    /// <summary>
    /// Process IL instructions for assembly.
    /// </summary>
    public class AsyncInstructionManager
    {
        public List<MethodInfo> AsyncMethodInfo { get; }

        /**************************************************************************************/

        public AsyncInstructionManager(string assemmblyPath)
        {
            AsyncMethodInfo = new List<MethodInfo>();
            var methods = ReadAsyncMethodsFromAssembly(assemmblyPath);
            ProcessInstructions(methods);
        }

        /**************************************************************************************/

        /// <summary>
        ///Skip compiler generated code at the beggining of method and find start index for processing.
        /// </summary>
        ///<param name="body">Method body</param>
        ///<returns> Start index for processing.</returns>
        private int CalcStartIndex(MethodBody body)
        {
            var startIndex = 0;
            while (startIndex < body.Instructions.Count())
            {
                if (IsNopCheck(body.Instructions[startIndex]))
                {
                    startIndex++;
                    break;
                }
                startIndex++;
            }
            return startIndex;
        }

        /// <summary>
        ///Check if instruction starts try block and skip generated code at the beggining.
        /// </summary>
        ///<param name="body">Method body</param>
        ///<param name="processedInfo">Method info</param>
        ///<param name="currentInst">Idex of current instruction</param>
        private void CheckStartTryBlock(MethodBody body, ref MethodInfo processedInfo, ref int currentInst)
        {
            var index= currentInst;
            if (body.ExceptionHandlers.Any(_=>_.TryStart== body.Instructions[index]))
            {
                processedInfo.Instructions.Add(new InstructionData(body.Instructions[currentInst], false));
                currentInst++;

                while (currentInst < body.Instructions.Count && !IsNopCheck(body.Instructions[currentInst]))
                {
                    processedInfo.Instructions.Add(new InstructionData(body.Instructions[currentInst], false));
                    currentInst++;
                }
            }
        }

        /// <summary>
        ///Check if instruction starts generated handler block (last handler block) and skip it.
        /// </summary>
        ///<param name="body">Method body</param>
        ///<param name="processedInfo">Method info</param>
        ///<param name="currentInst">Idex of current instruction</param>
        private void CheckGenHandlerBlock(MethodBody body, ref MethodInfo processedInfo, ref int currentInst)
        {
            var index = currentInst;
            if (body.ExceptionHandlers.LastOrDefault()?.HandlerStart == body.Instructions[index])
            {
                while (currentInst < body.Instructions.Count && body.ExceptionHandlers.LastOrDefault()?.HandlerEnd != body.Instructions[currentInst])
                {
                    processedInfo.Instructions.Add(new InstructionData(body.Instructions[currentInst], false));
                    currentInst++;
                }
                while (currentInst < body.Instructions.Count && !IsNopCheck(body.Instructions[currentInst]))
                {
                    processedInfo.Instructions.Add(new InstructionData(body.Instructions[currentInst], false));
                    currentInst++;
                }
            }
        }

        /// <summary>
        ///Read MoveNext methods from the assembly.
        /// </summary>
        ///<param name="assemmblyPath">Path to assembly</param>
        private List<MethodDefinition> ReadAsyncMethodsFromAssembly(string assemmblyPath)
        {
            var instructions = new List<InstructionData>();
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(assemmblyPath);
            return assembly.MainModule
                    .GetTypes()
                    .SelectMany(t => t.Methods)
                    .Where(m => m.HasBody && Regex.IsMatch(m.FullName, $@"<\w+>d__\d+::MoveNext()"))
                    .ToList();
        }

        /// <summary>
        ///Check if instruction starts try block and skip generated code at the beggining.
        /// </summary>
        ///<param name="methods">List of <see cref="MethodDefinition"/></param>
        private void ProcessInstructions(List<MethodDefinition> methods)
        {
            foreach (var method in methods)
            {
                Console.WriteLine($"Process \tType = {method.DeclaringType.Name}\n\t\tMethod = {method.Name}");

                var methodInfo=(new MethodInfo(method.FullName));
                var startIndex = CalcStartIndex(method.Body);

                for (var i = 0; i < method.Body.Instructions.Count; i++)
                {
                    if (i < startIndex)
                    {
                       methodInfo.Instructions.Add(new InstructionData(method.Body.Instructions[i], false));
                    }
                    else
                    {
                        CheckStartTryBlock(method.Body, ref methodInfo, ref i);
                        CheckGenHandlerBlock(method.Body, ref methodInfo, ref i);
                        UserInstCheck(method.Body.Instructions.ToList(), ref methodInfo, ref i);
                    }
                }
                AsyncMethodInfo.Add(methodInfo);
                var instrs = methodInfo.GetUserInstructions();
                Console.WriteLine("Done.");
            }
        }

        /// <summary>
        ///Check if instruction is Nop.
        /// </summary>
        ///<param name="inst">Instruction</param>
        ///<returns> true if it is Nop and false in other cases</returns>
        private bool IsNopCheck(Instruction inst)
        {
            return inst.OpCode.Code == Code.Nop;
        }

        /// <summary>
        ///Check if instruction is Leave.
        /// </summary>
        ///<param name="inst">Instruction</param>
        ///<returns> true if it is Leave and false in other cases</returns>
        private bool IsLeaveCheck(Instruction inst)
        {
            return inst.OpCode.Code == Code.Leave || inst.OpCode.Code == Code.Leave_S;
        }

        /// <summary>
        ///Check if instruction is Br.
        /// </summary>
        ///<param name="inst">Instruction</param>
        ///<returns> true if it is Br and false in other cases</returns>
        private bool IsBrCheck(Instruction inst)
        {
            return inst.OpCode.Code == Code.Br || inst.OpCode.Code == Code.Br_S;
        }

        /// <summary>
        ///Check if a instruction is user instruction.
        /// </summary>
        public void UserInstCheck(List<Instruction> instList, ref MethodInfo processedInfo, ref int currentInst)
        {
            //process Nop
            if (IsNopCheck(instList[currentInst]))
                processedInfo.Instructions.Add(new InstructionData(instList[currentInst], false));

            //process Leave
            else if (IsLeaveCheck(instList[currentInst]))
                processedInfo.Instructions.Add(new InstructionData(instList[currentInst], false));

            //process IsCompleted-GetResult blocks
            else if (instList[currentInst].Operand!=null && instList[currentInst].Operand.ToString().Contains("::get_IsCompleted()"))
            {
                processedInfo.Instructions.Last().IsUserInst = false;
                while(currentInst < instList.Count() &&
                    instList[currentInst].Previous.Operand == null || !instList[currentInst].Previous.Operand.ToString().Contains("::GetResult()"))
                {
                    processedInfo.Instructions.Add(new InstructionData(instList[currentInst], false));
                    currentInst++;
                }

                while (currentInst < instList.Count() &&
                    (IsNopCheck(instList[currentInst]) || IsLeaveCheck(instList[currentInst])|| IsBrCheck(instList[currentInst])))
                {
                    processedInfo.Instructions.Add(new InstructionData(instList[currentInst], false));
                    currentInst++;
                }
                currentInst--;
            }

            else
                processedInfo.Instructions.Add(new InstructionData(instList[currentInst], true));
        }
    }
}
