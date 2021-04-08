using System;
using System.Collections.Generic;
using System.Linq;
using Drill4Net.Profiling.Tree;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    public record InjectorContext
    {
        public string ModuleName { get; set; }
        public string MethodFullName { get; set; }
        public InjectedMethod TreeMethod { get; set; }
        public InjectedType TreeType { get; set; }

        public ILProcessor Processor { get; set; }
        public Collection<Instruction> Instructions { get; }
        public Collection<ExceptionHandler> ExceptionHandlers { get; set; }
        public Instruction LastOperation { get; set; }
        public Stack<Instruction> IfStack { get; }

        public bool IsStrictEnterReturn { get; set; }

        public HashSet<Instruction> Jumpers { get; }
        public HashSet<Instruction> CompilerInstructions { get; }
        public HashSet<Instruction> Processed { get; }

        //public List<Instruction> Injections { get; }

        public MethodReference ProxyMethRef { get; set; }
        public Instruction LdstrReturn { get; set; }

        public string ReturnProbData { get; set; }

        public int CurIndex => _curIndex;
        public int _curIndex;

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
            IfStack = new Stack<Instruction>();
        }

        /***********************************************************************************************/

        public void SetIndex(int index)
        {
            if (index < 0)
                throw new ArgumentException("Index must greater zero");
            _curIndex = index;
        }

        public int IncrementIndex(int inc = 1)
        {
            _curIndex += inc;
            return _curIndex;
        }
    }
}
