using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Drill4Net.Common
{
    public class Pluginator
    {
        private readonly TypeChecker _typeChecker = new();

        /***********************************************************************/

        public List<Type> GetByInterface(string dir, Type plugBaseType, SourceFilterOptions filter = null)
        {
            if (string.IsNullOrWhiteSpace(dir))
                throw new ArgumentNullException(nameof(dir));
            if (!Directory.Exists(dir))
                throw new Exception($"Directory does not exists: [{dir}]");
            //
            ConcurrentDictionary<string, string> asms = new();
            return SearchByInterface(dir, plugBaseType, asms, filter);
        }

        internal List<Type> SearchByInterface(string dir, Type plugBaseType, ConcurrentDictionary<string, string> asms,
            SourceFilterOptions filter)
        {
            var list = new List<Type>();
            if (!filter.IsDirectoryNeed(dir))
                return list;
            var di = new DirectoryInfo(dir);
            if (!filter.IsFolderNeed(di.Parent.Name))
                return list;

            //files
            var files = Directory.GetFiles(dir)
                .Where(a => Path.GetExtension(a) == ".dll");
            Parallel.ForEach(files, (file) =>
            {
                if (!filter.IsFileNeedByPath(file))
                    return;
                if (!filter.IsFileNeed(Path.GetFileName(file)))
                    return;
                var dirTypes = GetTypes(file, plugBaseType, asms, filter);
                list.AddRange(dirTypes);
            });

            //subdirectories
            var dirs = Directory.GetDirectories(dir);
            Parallel.ForEach(dirs, (curDir) =>
            {
                var dirTypes = SearchByInterface(curDir, plugBaseType, asms, filter);
                list.AddRange(dirTypes);
            });

            return list;
        }

        internal IEnumerable<Type> GetTypes(string asmPath, Type plugBaseType, ConcurrentDictionary<string, string> asms,
            SourceFilterOptions filter)
        {
            if(!asms.TryAdd(Path.GetFileName(asmPath), null))
                return new List<Type>();
            //
            try
            {
                var assembly = Assembly.LoadFrom(asmPath);
                var types = assembly.GetTypes().Where(a => a.IsPublic);
                var list = new List<Type>();
                Parallel.ForEach(types, (type) =>
                {
                    #region Check
                    if (_typeChecker.IsSystemType(type.FullName))
                        return;
                    if (!filter.IsNamespaceNeed(type.Namespace))
                        return;
                    if (!filter.IsClassNeed(type.FullName))
                        return;
                    try
                    {
                        var attrs = type.GetCustomAttributes(true);
                        foreach (Attribute attr in attrs)
                        {
                            if (!filter.IsAttributeNeed(attr.GetType().Name))
                                return;
                        }
                    }
                    catch { } //it is may be normal
                    #endregion

                    if (type.IsSubclassOf(plugBaseType))
                        list.Add(type);
                });
                return list;
            }
            catch
            {
                return new List<Type>();
            }
        }
    }
}
