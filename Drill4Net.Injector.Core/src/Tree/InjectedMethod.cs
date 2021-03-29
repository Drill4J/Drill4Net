using System;
using System.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    [Serializable]
    public class InjectedMethod : InjectedEntity
    {
        public string Namespace { get; set; }
        public string ReturnType { get; set; }
        public string Parameters { get; set; }
        public string TypeName { get; set; }

        public string FromMethod { get; set; }

        public MethodSource SourceType { get; set; }

        /********************************************************************/

        public InjectedMethod(string typeName, string fullName, MethodSource sourceType)
        {
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            //
            var parts = GetParts(fullName);
            Namespace = parts.Ns;
            Name = parts.Name;
            ReturnType = parts.RetType;
            Parameters = parts.Pars;
            Fullname = fullName;
        }

        /********************************************************************/

        internal static (string Ns, string RetType, string Name, string Pars) GetParts(string fullName)
        {
            //TODO: regex !!! AAAAAAAAAA!!!!
            //System.String Drill4Net.Target.Common.AbstractGen`1::GetDesc(System.Boolean)
            string ns = null; string retType = null; 
            string name = null; string pars = null;
            if (string.IsNullOrWhiteSpace(fullName))
                return (ns, retType, name, pars);
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

            return (ns, retType, name, pars);
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
