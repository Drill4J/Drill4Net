using System;
using System.Collections.Generic;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class InjectedMethod : InjectedEntity
    {
        public string Namespace { get; set; }
        public string ReturnType { get; set; }
        public string Parameters { get; set; }
        public string TypeName { get; set; }

        public string FromMethod { get; set; }

        public string BusinessMethod => FromMethod ?? Fullname;
        public string BusinessType { get; set; }

        public MethodSource SourceType { get; set; }

        /********************************************************************/

        public InjectedMethod(string assemblyName, string typeName, string businessTypeName, string fullName, MethodSource sourceType)
        {
            AssemblyName = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            BusinessType = businessTypeName ?? throw new ArgumentNullException(nameof(businessTypeName));
            SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
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
            //TODO: regex !!! AAAAAAAAAA!!!!
            //System.String Drill4Net.Target.Common.AbstractGen`1::GetDesc(System.Boolean)
            string ns = null; string retType = null; 
            string name = null; string pars = null;
            if (string.IsNullOrWhiteSpace(fullName))
                return new ParsedMethod(ns, retType, name, pars);
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
            InjectedType type = null;
            InjectedSimpleEntity key = forEntity;
            while (true)
            {
                if (!parentMap.ContainsKey(key))
                    break;
                type = parentMap[key] as InjectedType;
                if (!type.IsCompilerGenerated)
                    break;
                key = type;
            }
            return type;
        }

        public override string ToString()
        {
            return $"M: {Fullname}";
        }
    }
}
