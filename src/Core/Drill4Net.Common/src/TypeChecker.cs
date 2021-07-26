using System;
using System.IO;
using System.Collections.Generic;

namespace Drill4Net.Common
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

        public bool IsSystemTypeByMethod(string methodFullName)
        {
            if (string.IsNullOrWhiteSpace(methodFullName))
                throw new ArgumentNullException(nameof(methodFullName));
            //
            if (IsAnonymousType(methodFullName)) //guanito
                return false;
            var type = CommonUtils.GetTypeByMethod(methodFullName);
            if (type == null)
                return false;
            return IsSystemType(type);
        }

        public bool IsAnonymousType(string sig)
        {
            if (string.IsNullOrWhiteSpace(sig))
                throw new ArgumentNullException(nameof(sig));
            return sig.Contains("<>f__AnonymousType");
        }


        public bool IsSystemType(string typeFullName)
        {
            var ns1 = CommonUtils.GetNamespace(typeFullName);
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
                "Accessibility",
                "PresentationCore",
                "PresentationFramework",
                "PresentationUI",
                "ReachFramework",
                "UIAutomationClient",
                "UIAutomationClientSideProviders",
                "UIAutomationProvider",
                "UIAutomationTypes",
                "WindowsFormsIntegration",
                "DirectWriteForwarder",
                //...
            };
            return hash;
        }
    }
}