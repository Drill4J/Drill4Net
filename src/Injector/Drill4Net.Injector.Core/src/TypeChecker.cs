using System.IO;
using System.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    public class TypeChecker
    {
        private readonly HashSet<string> _restrictNamespaces;
        
        /***********************************************************************/

        public TypeChecker()
        {
            _restrictNamespaces = GetRestrictNamespaces();
        }
        
        /***********************************************************************/

        public bool CheckByAssemblyPath(string filePath)
        {
            return !IsSystemType(Path.GetFileNameWithoutExtension(filePath));
        }
        
        public bool CheckByMethodFullName(string methodFullName)
        {
            var tAr = methodFullName.Split(' ')[1].Split(':');
            var type = tAr[0];
            return !IsSystemType(type);
        }
        
        public bool IsSystemType(string typeFullName)
        {
            var tAr = typeFullName.Split('.');
            var ns1 = tAr[0];
            return _restrictNamespaces.Contains(ns1);
        }

        private HashSet<string> GetRestrictNamespaces()
        {
            var hash = new HashSet<string>
            {
                "System",
                "Microsoft",
                "Windows",
                "WindowsBase",
                "FSharp",
                //...
            };
            return hash;
        }
    }
}