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
        public string ModuleName => TypeCtx.AssemblyCtx.Module.Name;
        public AssemblyContext AssemblyCtx => TypeCtx?.AssemblyCtx;

        public MethodDefinition Definition { get; }
        public InjectedMethod Method { get; }

        public TypeContext TypeCtx { get; }
        public InjectedType Type => TypeCtx.InjType;

        public int StartIndex { get; set; }
        public Collection<Instruction> Instructions { get; }
        public List<Instruction> OrigInstructions { get; }
        public HashSet<Instruction> BusinessInstructions { get; }
        public Instruction LastOperation { get; set; }
        public HashSet<Instruction> CompilerInstructions { get; }
        public HashSet<Instruction> AheadProcessed { get; }

        public ILProcessor Processor { get; }
        public Collection<ExceptionHandler> ExceptionHandlers { get; }
        public Stack<Instruction> IfStack { get; }
        public bool IsStrictEnterReturn { get; set; }
        public HashSet<object> FirstInjectInstructions { get; }
        public Dictionary<Instruction, Instruction> ReplacedJumps { get; }
        public HashSet<Instruction> Jumpers { get; }

        /// <summary>
        /// Set of an anchors (targets of <see cref="Jumpers"/>, in fact, their Operands)
        /// </summary>
        public HashSet<object> Anchors { get; }
  
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

        public MethodContext(TypeContext typeCtx, InjectedMethod method, MethodDefinition methodDef)
        {
            TypeCtx = typeCtx ?? throw new ArgumentNullException(nameof(typeCtx));
            Definition = methodDef ?? throw new ArgumentNullException(nameof(methodDef));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            var body = methodDef.Body;
            ExceptionHandlers = body.ExceptionHandlers;
            Processor = body.GetILProcessor();
            var instructions = body.Instructions;
            Instructions = instructions ?? throw new ArgumentNullException(nameof(instructions));
            OrigInstructions = instructions.ToList();
            OrigSize = instructions.Count;
            LastOperation = instructions.Last();
            //
            BusinessInstructions = new HashSet<Instruction>();
            AheadProcessed = new HashSet<Instruction>();
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
        public void SetPosition(int index)
        {
            if (index < 0)
                throw new ArgumentException("Index must greater zero");
            CurIndex = index;
            var origInd = OrigInstructions.IndexOf(Instructions[index]); 
            SourceIndex = origInd;
        }

        /// <summary>
        /// Increments <see cref="CurIndex"/> - due the injection
        /// </summary>
        /// <param name="inc">Increment for changing the <see cref="CurIndex"/></param>
        /// <returns></returns>
        public int CorrectIndex(int inc = 1)
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
