using System;

namespace Drill4Net.Injector.Core
{
    [Serializable]
    public class InjectedClass : InjectedEntity
    {
        public ClassSource SourceType { get; set; }

        /******************************************************************/

        public InjectedClass(string assemblyName, string fullName): 
            base(GetName(fullName), GetFullPath(assemblyName, fullName))
        {
            Fullname = fullName;
        }

        /******************************************************************/

        internal static string GetName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentNullException(nameof(fullName));
            var ar = fullName.Split('.');
            return ar[ar.Length - 1];
        }

        internal static string GetFullPath(string assemblyName, string fullName)
        {
            return string.IsNullOrWhiteSpace(assemblyName) ? fullName : $"{assemblyName};{fullName}";
        }

        public override string ToString()
        {
            return Fullname;
        }
    }
}
