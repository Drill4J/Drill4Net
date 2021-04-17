using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class InjectedMethod : InjectedEntity
    {
        public string Namespace { get; set; }
        public string ReturnType { get; set; }
        public string Parameters { get; set; }
        public string TypeName { get; set; }

        public bool IsCompilerGenerated => SourceType.MethodType == MethodType.CompilerGeneratedPart;

        public CompilerGeneratedInfo CGInfo { get; }

        public Dictionary<string, int> CalleeIndexes { get; set; }

        public string BusinessMethod => GetBusinessMethod();
        public string BusinessType { get; set; }

        public MethodSource SourceType { get; set; }
        
        /// <summary>
        /// Count of instructions in various 'business parts' of the IL code
        /// (including compiler generated classes and functions) at the own level
        /// of hierarchy of calls CG members
        /// </summary>
        public int BusinessSize { get; set; } = -1;
        
        /// <summary>
        /// Count of only own 'business parts' of the IL code
        /// </summary>
        public int OwnBusinessSize { get; set; } = -1;

        public IEnumerable<CrossPoint> Points => Filter(typeof(CrossPoint), false).Cast<CrossPoint>();
        
        /// <summary>
        /// Dictionary of code blocks: key is point Id, value - coverage part of code by this block
        /// </summary>
        public Dictionary<int, float> Blocks { get; }

        /********************************************************************/

        public InjectedMethod(string assemblyName, string typeName, string businessTypeName, string fullName, MethodSource sourceType)
        {
            AssemblyName = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            BusinessType = businessTypeName ?? throw new ArgumentNullException(nameof(businessTypeName));
            SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            if (sourceType.MethodType == MethodType.CompilerGeneratedPart)
                CGInfo = new CompilerGeneratedInfo();
            CalleeIndexes = new Dictionary<string, int>();
            Blocks = new Dictionary<int, float>();
            //
            var parts = GetParts(fullName);
            Namespace = parts.Namespace;
            Name = parts.Name;
            ReturnType = parts.Return;
            Parameters = parts.Parameters;
            Fullname = fullName;
        }

        /********************************************************************/

        internal static ParsedMethod GetParts(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return new ParsedMethod();
            
            //TODO: regex !!! AAAAAAAAAA!!!!
            //Example: System.String Drill4Net.Target.Common.AbstractGen`1::GetDesc(System.Boolean)
            string ns = null; string retType = null; 
            string name = null; string pars = null;
            //
            if (!fullName.Contains("::")) //it's exactly short name
            {
                name = fullName;
            }
            else
            {
                var s = fullName;
                if (fullName.Contains(" ")) //return param exists?
                {
                    var ar = s.Split(' ');
                    retType = ar[0];
                    s = ar[1];
                }
                var ar1 = s.Split(':');
                var nsAr = ar1[0].Split('.');
                for (var i = 0; i < nsAr.Length - 1; i++)
                    ns += nsAr[i] + ".";
                ns = ns?.Remove(ns.Length-1, 1);
                var ar2 = ar1[2].Split('(');
                name = ar2[0];
                pars = ar2[1];
                pars = pars.Length > 1 ? pars.Remove(pars.Length - 1, 1) : null;
            }

            return new ParsedMethod(ns, retType, name, pars);
        }

        public InjectedType FindBusinessType(Dictionary<InjectedSimpleEntity, InjectedSimpleEntity> parentMap,
            InjectedMethod forEntity)
        {
            if (parentMap == null)
                throw new ArgumentNullException(nameof(parentMap));
            if (forEntity == null)
                throw new ArgumentNullException(nameof(forEntity));
            //
            InjectedType type = null;
            InjectedSimpleEntity key = forEntity;
            while (true)
            {
                if (!parentMap.ContainsKey(key))
                    break;
                type = parentMap[key] as InjectedType;
                if (type is {IsCompilerGenerated: false})
                    break;
                key = type;
            }
            return type;
        }

        internal virtual string GetBusinessMethod()
        {
            var method = this;
            while (true)
            {
                var cgInfo = method?.CGInfo;
                if (cgInfo == null)
                    return method.Fullname;
                if (cgInfo.FromMethod != null)
                    return cgInfo.FromMethod;
                if (cgInfo.Caller == null)
                    return method.Fullname;
                method = cgInfo.Caller;
            }
        }

        public override string ToString()
        {
            return $"M: {Fullname}";
        }
    }
}
