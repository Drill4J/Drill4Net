using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.Profiling.Tree
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


        public IEnumerable<InjectedDirectory> GetAllDirectories()
        {
            return Flatten(typeof(InjectedType)) //exactly for class, not assembly
                .Where(a => a.GetType().Name == nameof(InjectedDirectory))
                .Cast<InjectedDirectory>();
        }

        public IEnumerable<InjectedAssembly> GetAllAssemblies()
        {
            return Flatten(typeof(InjectedType))
                .Where(a => a.GetType().Name == nameof(InjectedAssembly))
                .Cast<InjectedAssembly>();
        }


        public IEnumerable<InjectedAssembly> GetAssemblies()
        {
            return _children.Where(a => a.GetType().Name == nameof(InjectedAssembly))
                 .Cast<InjectedAssembly>();
        }

        public IEnumerable<InjectedType> GetAllTypes()
        {
            return Flatten(typeof(InjectedMethod))
                .Where(a => a.GetType().Name == nameof(InjectedType))
                .Cast<InjectedType>();
        }

        public InjectedAssembly GetAssembly(string fullName, bool inDeep = false)
        {
            if (!inDeep)
            {
                return GetAssemblies()
                    .Where(a => a.FullName.Equals(fullName, StringComparison.CurrentCultureIgnoreCase))
                    as InjectedAssembly;
            }
            else
            {
                var asms = GetAllAssemblies();
                return asms.FirstOrDefault(a => a.FullName == fullName);
            }
        }

        public IEnumerable<InjectedMethod> GetAllMethods()
        {
            return Flatten(typeof(CrossPoint))
                .Where(a => a.GetType().Name == nameof(InjectedMethod))
                .Cast<InjectedMethod>();
        }

        public IEnumerable<CrossPoint> GetAllPoints()
        {
            return Filter(typeof(CrossPoint), true).Cast<CrossPoint>();
        }

        public InjectedDirectory GetDirectory(string path)
        {
            return Flatten(typeof(InjectedType))
                .FirstOrDefault(a => a.Path.Equals(path, StringComparison.CurrentCultureIgnoreCase))
                as InjectedDirectory;
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
            return $"{base.ToString()}{Path}";
        }
    }
}
