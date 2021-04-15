using System;
using System.Collections.Generic;
using Mono.Cecil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class TypeContext
    {
        public AssemblyContext AssemblyCtx { get; set; }
        public TypeDefinition Definition { get; }
        public InjectedType InjType { get; }
        public List<MethodContext> MethodContexts { get; set; }

        /*****************************************************************************************/
        
        public TypeContext(AssemblyContext asmCtx, TypeDefinition typeDef, InjectedType injType)
        {
            AssemblyCtx = asmCtx ?? throw new ArgumentNullException(nameof(asmCtx));
            Definition = typeDef ?? throw new ArgumentNullException(nameof(typeDef));
            InjType = injType ?? throw new ArgumentNullException(nameof(injType)); 
            //
            MethodContexts = new List<MethodContext>();
        }
        
        /*****************************************************************************************/
        
        public override string ToString()
        {
            return InjType.ToString();
        }
    }
}