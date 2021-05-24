using System.Collections.Generic;
using Mono.Cecil;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;
using Drill4Net.Injector.Core;

namespace Drill4Net.Injector.Engine
{
    /// <summary>
    /// Type helper
    /// </summary>
    internal static class TypeHelper
    {
        private static readonly TypeChecker _typeChecker = new();

        /*************************************************************************************/

        /// <summary>
        /// Get filtered types of assembly by specified options
        /// </summary>
        /// <param name="allTypes"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        internal static IEnumerable<TypeDefinition> FilterTypes(IEnumerable<TypeDefinition> allTypes, SourceFilterOptions opts)
        {
            var res = new List<TypeDefinition>();
            foreach (var typeDef in allTypes)
            {
                var typeName = typeDef.Name;
                if (typeName == "<Module>")
                    continue;
                var typeFullName = typeDef.FullName;
                if (_typeChecker.IsSystemType(typeFullName)) //system's types not needed any way
                    continue;
                if (!opts.IsClassNeed(typeFullName))
                    continue;
                if (!opts.IsNamespaceNeed(typeDef.Namespace))
                    continue;

                //attributes
                var not = false;
                foreach (var attr in typeDef.CustomAttributes)
                {
                    if (!opts.IsNamespaceNeed(typeDef.Namespace))
                    {
                        not = true;
                        break;
                    }
                }
                if (not)
                    continue;
                //
                res.Add(typeDef);
            }
            return res;
        }

        /// <summary>
        /// Try to get real type name from compiler generated classes. Technically, 
        /// it will be upward search the "normal name" in the type parent's hierarchy.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static string TryGetRealTypeName(TypeDefinition type)
        {
            if (type?.DeclaringType == null)
                return type?.FullName;
            do type = type.DeclaringType;
                while ( 
                        type.DeclaringType != null && 
                        (type.Name.Contains("c__DisplayClass") || type.Name.Contains("<>"))
                      );
            return type?.FullName;
        }

        /// <summary>
        /// Retreive some metadata for type
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        internal static TypeSource CreateTypeSource(TypeDefinition def)
        {
            return new TypeSource
            {
                AccessType = GetAccessType(def),
                IsAbstract = def.IsAbstract,
                IsGeneric = def.IsGenericInstance,
                //IsStatic = ...,
                IsValueType = def.IsValueType,
                IsNested = def.IsNested
            };
        }

        /// <summary>
        /// Get access type of type
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        internal static AccessType GetAccessType(TypeDefinition def)
        {
            if (def.IsNestedPrivate)
                return AccessType.Private;
            if (def.IsPublic)
                return AccessType.Public;
            return AccessType.Internal;
        }
    }
}
