using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Drill4Net.Common
{
    /// <summary>
    /// Search some types in assemblies by filter
    /// </summary>
    public class TypeFinder
    {
        private readonly TypeChecker _typeChecker = new();

        /***********************************************************************/

        public List<Type> GetBy(TypeFinderMode finderMode, string dir, Type searchType, SourceFilterOptions filter = null)
        {
            if (string.IsNullOrWhiteSpace(dir))
                return new List<Type>();
            if (!Directory.Exists(dir))
                throw new Exception($"Directory does not exists: [{dir}]");
            //
            ConcurrentDictionary<string, string> asms = new();
            return SearchBy(finderMode, dir, searchType, asms, filter).ToList();
        }

        internal IEnumerable<Type> SearchBy(TypeFinderMode finderMode, string dir, Type searchType, ConcurrentDictionary<string, string> asms,
            SourceFilterOptions filter)
        {
            var list = new ConcurrentBag<Type>();
            if (filter?.IsDirectoryNeed(dir) == false)
                return list;
            var di = new DirectoryInfo(dir);
            if (filter?.IsFolderNeed(di.Parent.Name) == false)
                return list;

            //files
            var files = Directory.GetFiles(dir)
                .Where(a => Path.GetExtension(a) == ".dll");
            Parallel.ForEach(files, (file) =>
           //foreach (var file in files) //DEBUG!!!
            {
                if (filter?.IsFileNeedByPath(file) == false)
                    return;
                    //continue;
                if (filter?.IsFileNeed(Path.GetFileName(file)) == false)
                    return;
                    //continue;
                //
                var dirTypes = GetTypes(finderMode, file, searchType, asms, filter);
                foreach(var type in dirTypes)
                    list.Add(type);
            });
            //}

            //subdirectories
            var dirs = Directory.GetDirectories(dir);
            Parallel.ForEach(dirs, (curDir) =>
            //foreach (var curDir in dirs) //DEBUG!!!
            {
                var dirTypes = SearchBy(finderMode, curDir, searchType, asms, filter);
                foreach (var type in dirTypes)
                    list.Add(type);
            });
            //}

            return list;
        }

        internal IEnumerable<Type> GetTypes(TypeFinderMode finderMode, string asmPath, Type searchType, ConcurrentDictionary<string, string> asms,
            SourceFilterOptions filter)
        {
            if (!asms.TryAdd(Path.GetFileName(asmPath), null))
                return new ConcurrentBag<Type>();
            //
            try
            {
                //CommonUtils.WriteTempLog($"{nameof(TypeFinder)}|GetTypes|Asm: [{asmPath}]");
                var assembly = Assembly.LoadFrom(asmPath);
                var types = assembly.GetTypes().Where(a => a.IsPublic);
                var list = new ConcurrentBag<Type>();
                Parallel.ForEach(types, (type) =>
                //foreach (var type in types) //DEBUG
                {
                    #region Check
                    if (_typeChecker.IsSystemType(type.FullName))
                        return;
                        //continue;
                    if (filter?.IsNamespaceNeed(type.Namespace) == false)
                        return;
                        //continue;
                    if (filter?.IsClassNeed(type.FullName) == false)
                        return;
                        //continue;
                    try
                    {
                        var attrs = type.GetCustomAttributes(true);
                        foreach (Attribute attr in attrs)
                        {
                            if (filter?.IsAttributeNeed(attr.GetType().Name) == false)
                                //return;
                                continue;
                        }
                    }
                    catch { } //it is may be normal
                    #endregion

                    switch (finderMode)
                    {
                        case TypeFinderMode.ClassChildren:
                            if (type.IsSubclassOf(searchType))
                                list.Add(type);
                            break;
                        case TypeFinderMode.Interface:
                            var interfaces = type.GetInterfaces();
                            var intrfs = Array.Find(interfaces, a => a.Name == searchType.Name);
                            if (intrfs != null)
                                list.Add(type);
                            break;
                        case TypeFinderMode.Attribute:
                            var attrs = type.GetCustomAttributes();
                            var attr = attrs.FirstOrDefault(a => a.GetType().Name == searchType.Name);
                            if (attr != null)
                                list.Add(type);
                            break;
                        default:
                            throw new Exception($"Unknown search type: {finderMode}");
                    }
                });
                //}
                return list;
            }
            catch
            {
                return new ConcurrentBag<Type>();
            }
        }
    }
}
