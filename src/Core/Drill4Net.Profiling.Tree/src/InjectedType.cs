using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class InjectedType : InjectedEntity
    {
        public string Namespace { get; set; }

        public bool IsCompilerGenerated => BusinessType != FullName || FullName.StartsWith("<>");

        public string BusinessType { get; set; }

        /// <summary>
        /// For compiler generated classes which was created 
        /// compiler for some business methods
        /// </summary>
        public string FromMethod { get; set; }

        public TypeSource Source { get; set; }

        /**************************************************************************************/

        public InjectedType(string assemblyName, string fullName, string businessName = null) : 
            base(assemblyName, GetName(fullName), GetSource(assemblyName, fullName))
        {
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
            var lastPointInd = FullName.LastIndexOf(".");
            if(lastPointInd != -1)
                Namespace = FullName.Substring(0, lastPointInd);
            BusinessType = businessName ?? GetBusinessType(fullName);
        }

        /**************************************************************************************/

        internal string GetBusinessType(string fullName)
        {
            //TODO: regex!!!
            var list = fullName.Split('/').ToList();
            for (var i = 0; i < list.Count; i++)
            {
                var a = list[i];
                if (!a.StartsWith("<>") && !a.Contains(">d__"))
                    continue;
                var c = list.Count - i;
                for (var j = 0; j < c; j++)
                    list.RemoveAt(list.Count - 1);
                break;
            }
            return string.Join("/", list);
        }
        
        public IEnumerable<InjectedType> GetNestedTypes()
        {
            return _children.Where(a => a.GetType().Name == nameof(InjectedType))
                .Cast<InjectedType>();
        }
        
        public IEnumerable<InjectedMethod> GetMethods()
        {
            return _children.Where(a => a.GetType().Name == nameof(InjectedMethod))
                .Cast<InjectedMethod>();
        }

        internal static string GetName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentNullException(nameof(fullName));
            var ar = fullName.Split('.');
            return ar[ar.Length - 1];
        }

        internal static string GetSource(string assemblyName, string fullName)
        {
            return string.IsNullOrWhiteSpace(assemblyName) ? fullName : $"{assemblyName};{fullName}";
        }

        public override string ToString()
        {
            return $"{base.ToString()}T: {FullName}";
        }
    }
}
