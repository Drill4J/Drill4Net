using System;
using System.Collections.Generic;
using System.Linq;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Agent.Standard
{
    public class TreeConverter
    {
        #region AstEntities
        public List<AstEntity> ToAstEntities(IEnumerable<InjectedType> injTypes)
        {
            if (injTypes == null)
                throw new ArgumentNullException(nameof(injTypes));
            //
            var res = new List<AstEntity>();
            foreach (var type in injTypes.AsParallel())
            {
                lock (res)
                    res.Add(ToAstEntity(type));
            }
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
                entity.methods.Add(ToAstMethod(injMethod));
            }
            return entity;
        }
        
        internal AstMethod ToAstMethod(InjectedMethod injMethod)
        {
            if (injMethod == null)
                throw new ArgumentNullException(nameof(injMethod));
            //
            var astMethod = new AstMethod(injMethod.Name, injMethod.ReturnType, injMethod.OwnBusinessSize, injMethod.Source.HashCode);
            if (injMethod.Parameters != null)
                astMethod.@params = injMethod.Parameters.Split(',').Select(a => a.Trim()).ToList();
            return astMethod;
        }
        #endregion
        #region ExecClassData
        public CoverageDispatcher CreateCoverageDispatcher(StartSessionPayload session, IEnumerable<InjectedType> injTypes)
        {
            //TODO: cloning from Template object?
            var disp = new CoverageDispatcher(session); //TODO: bind the session!!!!!
            string testName = session?.TestName ?? $"{AgentConstants.TEST_NAME_DEFAULT}_{Guid.NewGuid()}";
            foreach (var type in injTypes.AsParallel())
            {
                var typeName = type.FullName;
                var data = new ExecClassData(testName, typeName);
                var methods = type.GetMethods()?.ToList();
                if (methods?.Any() != true) 
                    continue;
                var ind = 0;
                foreach (var meth in methods) //don't parallel here!
                {
                    var inds = meth.Coverage.PointUidToEndIndex;
                    foreach (var pointUid in inds.Keys)
                    {
                        var localEnd = inds[pointUid];
                        var start = ind;
                        var end = start + localEnd;
                        disp.AddPoint(pointUid, data, start, end);
                        ind += localEnd + 1;
                    }
                }
                data.InitProbes(ind);//not needed +1
            }
            return disp;
        }
        #endregion
    }
}