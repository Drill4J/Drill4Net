using System;
using System.Collections.Generic;
using Mono.Cecil;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Injector.Core
{
    public class TypeContext
    {
        public AssemblyContext AssemblyCtx { get; }
        public TypeDefinition Definition { get; }
        public InjectedType InjType { get; }
        public Dictionary<string, MethodContext> MethodContexts { get; }

        /*****************************************************************************************/

        public TypeContext(AssemblyContext asmCtx, TypeDefinition typeDef, InjectedType injType)
        {
            AssemblyCtx = asmCtx ?? throw new ArgumentNullException(nameof(asmCtx));
            Definition = typeDef ?? throw new ArgumentNullException(nameof(typeDef));
            InjType = injType ?? throw new ArgumentNullException(nameof(injType)); 
            //
            MethodContexts = new Dictionary<string, MethodContext>();
        }
        
        /*****************************************************************************************/
        
        public override string ToString()
        {
            return InjType.ToString();
        }
    }
}