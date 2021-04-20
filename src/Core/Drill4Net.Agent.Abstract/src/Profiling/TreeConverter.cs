using System;
using System.Collections.Generic;
using System.Linq;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Agent.Abstract
{
    public class TreeConverter
    {
        public IEnumerable<AstEntity> ToAstEntities(InjectedSolution tree)
        {
            if (tree == null)
                throw new ArgumentNullException(nameof(tree));
            var injTypes = tree.GetAllTypes()
                .Where(a => !a.IsCompilerGenerated);
            var res = new List<AstEntity>();
            foreach (var type in injTypes)
                res.Add( ToAstEntity(type));
            return res;
        }
        
        internal AstEntity ToAstEntity(InjectedType injType)
        {
            if (injType == null)
                throw new ArgumentNullException(nameof(injType));
            //
            var entity = new AstEntity(injType.Path, injType.BusinessType);
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