using System;
using System.Linq;

namespace Drill4Net.Injector.Core
{
    [Serializable]
    public class InjectedType : InjectedEntity
    {
        public bool IsCompilerGenerated { get; set; }

        public string BusinessType { get; set; }

        /// <summary>
        /// For compiler generated classes which was created 
        /// compiler for some business methods
        /// </summary>
        public string FromMethod { get; set; }

        public ClassSource SourceType { get; set; }

        /******************************************************************/

        public InjectedType(string assemblyName, string fullName): 
            base(GetName(fullName), GetSource(assemblyName, fullName))
        {
            Fullname = fullName;
            BusinessType = GetBusinessType(fullName);
            IsCompilerGenerated = fullName.Contains("<>");
        }

        /******************************************************************/

        internal string GetBusinessType(string fullName)
        {
            //TODO: regex!!!
            var list = fullName.Split('/').ToList();
            for(var i = 0; i < list.Count; i++)
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
            return $"T: {Fullname}";
        }
    }
}
