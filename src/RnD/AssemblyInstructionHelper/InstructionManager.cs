using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Drill4Net.AssemblyInstruction.Helper
{ 
    public class AsyncInstructionManager
    {
        public List<MethodInfo> AsyncMethodInfo { get; }

        //**************************************************************************************//

        public AsyncInstructionManager(string assemmblyPath)
        {
            AsyncMethodInfo = new List<MethodInfo>();
            var methods = ReadAsyncMethodsFromAssembly(assemmblyPath);
            ProcessInstructions(methods);
        }

        //**************************************************************************************//

        private int CalcStartIndex(MethodBody body)
        {
            var startIndex = 0;
            while (startIndex < body.Instructions.Count())
            {
                if (IsNopeCheck(body.Instructions[startIndex]))
                {
                    startIndex++;
                    break;
                }
                startIndex++;
            }
            return startIndex;
        }

        private void CheckStartTryBlock(MethodBody body, ref MethodInfo processedInfo, ref int currentInst)
        {
            var index= currentInst;
            if (body.ExceptionHandlers.Any(_=>_.TryStart== body.Instructions[index]))
            {
                processedInfo.Instructions.Add(new InstructionData(body.Instructions[currentInst], false));
                currentInst++;

                while (currentInst < body.Instructions.Count && !IsNopeCheck(body.Instructions[currentInst]))
                {
                    processedInfo.Instructions.Add(new InstructionData(body.Instructions[currentInst], false));
                    currentInst++;
                }
            }
        }

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
                while (currentInst < body.Instructions.Count && !IsNopeCheck(body.Instructions[currentInst]))
                {
                    processedInfo.Instructions.Add(new InstructionData(body.Instructions[currentInst], false));
                    currentInst++;
                }
            }
        }

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
                var test = methodInfo.GetUserInstruction();
                Console.WriteLine("Done.");
            }
        }

        private bool IsNopeCheck(Instruction inst)
        {
            return inst.OpCode.Code == Code.Nop;
        }

        private bool IsLeaveCheck(Instruction inst)
        {
            return inst.OpCode.Code == Code.Leave || inst.OpCode.Code == Code.Leave_S;
        }

        private bool IsBrCheck(Instruction inst)
        {
            return inst.OpCode.Code == Code.Br || inst.OpCode.Code == Code.Br_S;
        }

        public void UserInstCheck(List<Instruction> instList, ref MethodInfo processedInfo, ref int currentInst)
        {
            //process Nop
            if (IsNopeCheck(instList[currentInst]))
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
                    (IsNopeCheck(instList[currentInst]) || IsLeaveCheck(instList[currentInst])|| IsBrCheck(instList[currentInst])))
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
