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

        public bool CheckByPath(string filePath)
        {
            var ns1 = Path.GetFileNameWithoutExtension(filePath).Split('.')[0];
            return !_restrictNamespaces.Contains(ns1);
        }
        
        public bool CheckByTypeName(string typeFullName)
        {
            var tAr = typeFullName.Split('.');
            var ns1 = tAr[0];
            return !_restrictNamespaces.Contains(ns1);
        }
        
        public bool CheckByMethodName(string methodFullName)
        {
            var tAr = methodFullName.Split(' ')[1].Split(':');
            var type = tAr[0];
            return CheckByTypeName(type);
        }

        private HashSet<string> GetRestrictNamespaces()
        {
            var hash = new HashSet<string>
            {
                "Microsoft",
                "Windows",
                "System",
                "FSharp"
                //...
            };
            return hash;
        }
    }
}