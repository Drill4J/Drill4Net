using System;
using System.Collections.Generic;
using Drill4Net.Profiling.Tree;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    public record InjectorContext
    {
        public string ModuleName { get; }
        public string MethodFullName { get; }
        public InjectedMethod TreeMethod { get; set; }
        public InjectedType TreeType { get; set; }

        public ILProcessor Processor { get; }
        public Collection<Instruction> Instructions { get; }
        public Collection<ExceptionHandler> ExceptionHandlers { get; set; }
        public Instruction LastOperation { get; set; }
        public Stack<Instruction> IfStack { get; }

        public bool IsStrictEnterReturn { get; set; }

        public HashSet<Instruction> Jumpers { get; }
        
        /// <summary>
        /// Set of an anchor (target of jumpers, it's Operand)
        /// </summary>
        public HashSet<object> Anchors { get; }
        public HashSet<Instruction> CompilerInstructions { get; }
        public HashSet<Instruction> Processed { get; }

        public MethodReference ProxyMethRef { get; set; }

        public int CurIndex { get; private set; }

        /***********************************************************************************************/

        public InjectorContext(string moduleName, string methodFullName, Collection<Instruction> instructions,
            ILProcessor processor)
        {
            ModuleName = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
            MethodFullName = methodFullName ?? throw new ArgumentNullException(nameof(methodFullName));
            Instructions = instructions ?? throw new ArgumentNullException(nameof(instructions));
            Processor = processor ?? throw new ArgumentNullException(nameof(processor));
            //
            Processed = new HashSet<Instruction>();
            //Injections = new List<Instruction>();
            CompilerInstructions = new HashSet<Instruction>();
            Jumpers = new HashSet<Instruction>();
            Anchors = new HashSet<object> ();
            IfStack = new Stack<Instruction>();
        }

        /***********************************************************************************************/

        public void SetIndex(int index)
        {
            if (index < 0)
                throw new ArgumentException("Index must greater zero");
            CurIndex = index;
        }

        public int IncrementIndex(int inc = 1)
        {
            CurIndex += inc;
            return CurIndex;
        }
    }
}
