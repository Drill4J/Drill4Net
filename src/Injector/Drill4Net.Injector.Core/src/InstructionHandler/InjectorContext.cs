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
        public string ProxyNamespace { get; set; }
        public string ModuleName { get; }
        public string MethodFullName { get; }
        public InjectedMethod TreeMethod { get; set; }
        public InjectedType TreeType { get; set; }

        public ILProcessor Processor { get; }
        public Collection<Instruction> Instructions { get; }
        public Collection<ExceptionHandler> ExceptionHandlers { get; set; }
        public Instruction LastOperation { get; }
        public Stack<Instruction> IfStack { get; }

        public bool IsStrictEnterReturn { get; set; }

        public HashSet<object> FirstInjectInstructions { get; }
        public Dictionary<Instruction, Instruction> ReplacedJumps { get; }
        public HashSet<Instruction> Jumpers { get; }
        
        /// <summary>
        /// Set of an anchors (targets of <see cref="Jumpers"/>, in fact, their Operands)
        /// </summary>
        public HashSet<object> Anchors { get; }
        public HashSet<Instruction> CompilerInstructions { get; }
        public HashSet<Instruction> Processed { get; }

        public MethodReference ProxyMethRef { get; set; }

        /// <summary>
        /// Real current instruction index for <see cref="Instructions"/>,
        /// taking into account the performed injections
        /// </summary>
        public int CurIndex { get; private set; }

        public Instruction CurInstruction => 
            CurIndex > 0 && CurIndex < Instructions.Count ? 
                Instructions[CurIndex] : 
                throw new ArgumentOutOfRangeException($"CurIndex must be in range of Instruction collection");
        
        /// <summary>
        /// Current instruction index from source IL code
        /// </summary>
        public int SourceIndex { get; private set; }

        /***********************************************************************************************/

        public InjectorContext(string moduleName, string methodFullName, Collection<Instruction> instructions,
            ILProcessor processor)
        {
            ModuleName = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
            MethodFullName = methodFullName ?? throw new ArgumentNullException(nameof(methodFullName));
            Instructions = instructions ?? throw new ArgumentNullException(nameof(instructions));
            Processor = processor ?? throw new ArgumentNullException(nameof(processor));
            LastOperation = instructions.Last();
            //
            Processed = new HashSet<Instruction>();
            FirstInjectInstructions = new HashSet<object>();
            CompilerInstructions = new HashSet<Instruction>();
            ReplacedJumps = new Dictionary<Instruction, Instruction>();
            Jumpers = new HashSet<Instruction>();
            Anchors = new HashSet<object> ();
            IfStack = new Stack<Instruction>();
        }

        /***********************************************************************************************/

        /// <summary>
        /// Set value for both <see cref="SourceIndex"/> and <see cref="CurIndex"/>
        /// </summary>
        /// <param name="index">Value of indexes</param>
        /// <exception cref="ArgumentException"></exception>
        public void SetIndex(int index)
        {
            if (index < 0)
                throw new ArgumentException("Index must greater zero");
            CurIndex = index;
            SourceIndex = index;
        }

        /// <summary>
        /// Increments <see cref="CurIndex"/> - due the injection
        /// </summary>
        /// <param name="inc">Increment for changing the <see cref="CurIndex"/></param>
        /// <returns></returns>
        public int IncrementIndex(int inc = 1)
        {
            CurIndex += inc;
            return CurIndex;
        }
    }
}
