using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public record MethodContext
    {
        public TypeContext TypeCtx { get; }
        public string ModuleName => TypeCtx.AssemblyCtx.Module.Name;
        public InjectedType Type { get; set; }

        public MethodDefinition Definition { get; }
        public InjectedMethod Method { get; set; }

        public ILProcessor Processor { get; }
        public Collection<Instruction> Instructions { get; }
        public HashSet<Instruction> BusinessInstructions { get; }
        public int StartIndex { get; set; }
        public Collection<ExceptionHandler> ExceptionHandlers { get; set; }
        public Instruction LastOperation { get; set; }
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

        public string ProxyNamespace { get; set; }
        public MethodReference ProxyMethRef { get; set; }
        
        /// <summary>
        /// Current instruction index from source IL code
        /// </summary>
        public int SourceIndex { get; private set; }

        /// <summary>
        /// Real current instruction index for <see cref="Instructions"/>,
        /// taking into account the performed injections
        /// </summary>
        public int CurIndex { get; private set; }

        public int OrigSize { get; }

        public Instruction CurInstruction => 
            CurIndex >= 0 && CurIndex < Instructions.Count ? 
                Instructions[CurIndex] : 
                throw new ArgumentOutOfRangeException($"CurIndex must be in range of Instruction collection");

        /***********************************************************************************************/

        public MethodContext(TypeContext typeCtx, MethodDefinition methodDef, Collection<Instruction> instructions, ILProcessor processor)
        {
            TypeCtx = typeCtx ?? throw new ArgumentNullException(nameof(typeCtx));
            Definition = methodDef ?? throw new ArgumentNullException(nameof(methodDef));
            Instructions = instructions ?? throw new ArgumentNullException(nameof(instructions));
            Processor = processor ?? throw new ArgumentNullException(nameof(processor));
            OrigSize = instructions.Count;
            LastOperation = instructions.Last();
            //
            BusinessInstructions = new HashSet<Instruction>();
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
        
        public override string ToString()
        {
            return Method.ToString();
        }
    }
}
