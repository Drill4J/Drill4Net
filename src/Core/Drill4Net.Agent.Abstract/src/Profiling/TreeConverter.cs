using System;
using System.Collections.Generic;
using System.Linq;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Agent.Abstract
{
    public class TreeConverter
    {
        public List<AstEntity> ToAstEntities(InjectedSolution tree)
        {
            if (tree == null)
                throw new ArgumentNullException(nameof(tree));

            IEnumerable<InjectedType> injTypes = null;

            // check for different compiling target version 
            //we need only one for current runtime
            var rootDirs = tree.GetDirectories();
            if (rootDirs.Count() > 1)
            {
                var asmNameByDirs = (from dir in rootDirs
                                     select dir.GetAssemblies()
                                               .Select(a => a.Name)
                                               .Where(a => a.EndsWith(".dll"))
                                               .ToList())
                                     .ToList();
                if (asmNameByDirs[0].Count > 0)
                {
                    var multi = true;
                    for (var i = 1; i < asmNameByDirs.Count; i++)
                    {
                        var prev = asmNameByDirs[i - 1];
                        var cur = asmNameByDirs[i];
                        if (prev.Count != cur.Count || prev.Intersect(cur).Count() == 0)
                        {
                            multi = false;
                            break;
                        }
                    }
                    if (multi) //here many copies of target for diferent runtimes
                    {
                        var execVer = CommonUtils.GetEntryTargetVersioning();
                        InjectedDirectory targetDir = null;
                        foreach (var dir in rootDirs)
                        {
                            var asms = dir.GetAssemblies().ToList();
                            if (asms[0].Version.Version != execVer.Version)
                                continue;
                            targetDir = dir;
                            break;
                        }
                        injTypes = targetDir.GetAssemblies().SelectMany(a => a.GetAllTypes());
                    }
                }
            }
            else
            {
                injTypes = tree.GetAllTypes();
            }
            injTypes = injTypes.Where(a => !a.IsCompilerGenerated);
            //
            var res = new List<AstEntity>();
            foreach (var type in injTypes)
                res.Add(ToAstEntity(type));
            return res;
        }
        
        internal AstEntity ToAstEntity(InjectedType injType)
        {
            if (injType == null)
                throw new ArgumentNullException(nameof(injType));
            //
            var entity = new AstEntity(injType.Namespace, injType.Name);
            var injMethods = injType.GetMethods()
                .Where(a => !a.IsCompilerGenerated);
            foreach (var injMethod in injMethods)
            {
                entity.Methods.Add(ToAstMethod(injMethod));
            }
            return entity;
        }
        
        internal AstMethod ToAstMethod(InjectedMethod injMethod)
        {
            if (injMethod == null)
                throw new ArgumentNullException(nameof(injMethod));
            //
            var astMethod = new AstMethod(injMethod.Namespace, injMethod.Name, injMethod.ReturnType,
                0, injMethod.Source.HashCode);
            if (injMethod.Parameters != null)
                astMethod.Params = injMethod.Parameters.Split(',').Select(a => a.Trim()).ToList();
            return astMethod;
        }
    }
}