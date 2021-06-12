﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Method's data context
    /// </summary>
    public record MethodContext
    {
        public string ModuleName => AssemblyCtx?.Module?.Name;
        public AssemblyContext AssemblyCtx => TypeCtx?.AssemblyCtx;

        public MethodDefinition Definition { get; }
        public InjectedMethod Method { get; }

        public TypeContext TypeCtx { get; }
        public InjectedType Type => TypeCtx.InjType;

        /// <summary>
        /// Start index for list of instructions (we need sometimes to skip some compiler generated ones)
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// Current list of instructions including injected ones
        /// </summary>
        public Collection<Instruction> Instructions { get; }

        /// <summary>
        /// Original (not changed) list of method's instructions
        /// </summary>
        public List<Instruction> OrigInstructions { get; }

        /// <summary>
        /// Exactly business instructions
        /// </summary>
        public HashSet<Instruction> BusinessInstructions { get; }

        /// <summary>
        /// Last instruction of the method
        /// </summary>
        public Instruction LastOperation { get; set; }

        /// <summary>
        /// These instructions do not apply to the business code itself
        /// </summary>
        public HashSet<Instruction> CompilerInstructions { get; }

        /// <summary>
        /// Already ahead processed instruction
        /// </summary>
        public HashSet<Instruction> AheadProcessed { get; }

        /// <summary>
        /// Method's processor for the IL instructions
        /// </summary>
        public ILProcessor Processor { get; }

        /// <summary>
        /// List of exception handlers in the method (try/catch/finally)
        /// </summary>
        public Collection<ExceptionHandler> ExceptionHandlers { get; }

        /// <summary>
        /// Current stack of the If/Else instructions
        /// </summary>
        public Stack<Instruction> IfStack { get; }

        /// <summary>
        /// Do we need restrict Enter and Return cross-point's injection?
        /// </summary>
        public bool IsStrictEnterReturn { get; set; }

        /// <summary>
        /// The set of the starting injected instructions in the injecting block. Now it's just type is OpCodes.Ldstr
        /// </summary>
        public HashSet<object> StatingInjectInstructions { get; }

        /// <summary>
        /// Caching list of the replaced jumper's targets
        /// </summary>
        public Dictionary<Instruction, Instruction> ReplacedJumps { get; }

        /// <summary>
        /// Set of the jump-instructions for the method
        /// </summary>
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

        /// <summary>
        /// Original count of method's instructions
        /// </summary>
        public int OrigSize { get; }

        /// <summary>
        /// Current instuction according <see cref="CurIndex"/>
        /// </summary>
        public Instruction CurInstruction => 
            CurIndex >= 0 && CurIndex < Instructions.Count ? 
                Instructions[CurIndex] : 
                throw new ArgumentOutOfRangeException($"CurIndex must be in range of Instruction collection");

        /***********************************************************************************************/

        /// <summary>
        /// Create method's context
        /// </summary>
        /// <param name="typeCtx">Context of method's type</param>
        /// <param name="method">Method's injected info</param>
        /// <param name="methodDef">Method's definition</param>
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
            StatingInjectInstructions = new HashSet<object>();
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
            SourceIndex = OrigInstructions.IndexOf(Instructions[index]);
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
