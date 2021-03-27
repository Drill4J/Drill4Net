using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.Injector.Core
{
    [Serializable]
    public class InjectedDirectory : InjectedSimpleEntity
    {
        public string DestinationPath { get; set; }

        /*************************************************************************/

        public InjectedDirectory(string sourceDir, string destDir) : 
            base(GetLastFolder(sourceDir), sourceDir)
        {
            DestinationPath = destDir;
        }

        /*************************************************************************/

        public IEnumerable<InjectedDirectory> GetDirectories()
        {
            return _children.Where(a => a.GetType().Name == nameof(InjectedDirectory))
                 .Cast<InjectedDirectory>();
        }

        public IEnumerable<InjectedAssembly> GetAssemblies()
        {
            return _children.Where(a => a.GetType().Name == nameof(InjectedAssembly))
                 .Cast<InjectedAssembly>();
        }

        public InjectedAssembly GetAssembly(string fullName)
        {
            return GetAssemblies()
                .Where(a => a.Fullname.Equals(fullName, StringComparison.InvariantCultureIgnoreCase)) 
                as InjectedAssembly;
        }

        internal static string GetLastFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            else
            {
                var ar = path.Split('\\');
                var s = ar[ar.Length - 1];
                if (s != "")
                    return s;
                if (ar.Length > 1)
                    return ar[ar.Length - 2];
                else
                    return null;
            }
        }

        public override string ToString()
        {
            return Path;
        }
    }
}
