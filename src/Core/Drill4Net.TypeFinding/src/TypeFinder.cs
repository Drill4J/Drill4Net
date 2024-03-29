﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Drill4Net.Common;

namespace Drill4Net.TypeFinding
{
    /// <summary>
    /// Search some types in assemblies by filter
    /// </summary>
    public class TypeFinder
    {
        private readonly LibraryLoader _loader = new();
        private readonly TypeChecker _typeChecker = new();

        /***********************************************************************/

        /// <summary>
        /// Get types by filters in specified directory and search type
        /// </summary>
        /// <param name="finderMode"></param>
        /// <param name="dir"></param>
        /// <param name="searchType"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<Type> GetBy(TypeFinderMode finderMode, string dir, string searchType, SourceFilterOptions filter = null)
        {
            if (string.IsNullOrWhiteSpace(dir))
                return new List<Type>();
            if (!Directory.Exists(dir))
                throw new Exception($"Directory does not exists: [{dir}]");
            //
            ConcurrentDictionary<string, string> asms = new();
            return SearchBy(finderMode, dir, searchType, asms, filter).ToList();
        }

        internal IEnumerable<Type> SearchBy(TypeFinderMode finderMode, string dir, string searchType, ConcurrentDictionary<string, string> asms,
            SourceFilterOptions filter)
        {
            var list = new ConcurrentBag<Type>();
            if (filter?.IsDirectoryNeed(dir) == false)
                return list;
            var di = new DirectoryInfo(dir);
            var parentDir = di.Parent?.Name;
            if (!string.IsNullOrWhiteSpace(parentDir) && filter?.IsFolderNeed(parentDir) == false)
                return list;

            //files
            try
            {
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
                    if (dirTypes != null)
                        foreach (var type in dirTypes)
                            list.Add(type);
                });
                //}
            }
            catch { return list; }

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

        internal IEnumerable<Type> GetTypes(TypeFinderMode finderMode, string asmPath, string searchType, ConcurrentDictionary<string, string> asms,
            SourceFilterOptions filter)
        {
            if (!asms.TryAdd(Path.GetFileName(asmPath), null))
                return new ConcurrentBag<Type>();
            //
            try
            {
                using var assembly = _loader.LoadDefinition(asmPath);
                var types = assembly.MainModule.Types.Where(a => a.IsPublic);
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
                        foreach (var attr in type.CustomAttributes)
                        {
                            if (filter?.IsAttributeNeed(attr.GetType().Name) == false)
                                //continue;
                                return;
                        }
                    }
                    catch { } //it is may be normal
                    #endregion

                    switch (finderMode)
                    {
                        case TypeFinderMode.ClassChildren:
                            if (type.BaseType?.Name == searchType) //only 1 level of inheritance...
                                list.Add(_loader.LoadType(asmPath, type.FullName));
                            break;
                        case TypeFinderMode.Interface:
                            var interfaces = type.Interfaces;
                            var intrfs = interfaces.Where(a => a.InterfaceType.Name == searchType);
                            if (intrfs?.Any() == true)
                                list.Add(_loader.LoadType(asmPath, type.FullName));
                            break;
                        case TypeFinderMode.Attribute:
                            var attrs = type.CustomAttributes;
                            var attr = attrs.FirstOrDefault(a => a.AttributeType.Name == searchType);
                            if (attr != null)
                                list.Add(_loader.LoadType(asmPath, type.FullName));
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
