using System;
using System.Collections.Generic;
using System.Linq;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Abstract.Transfer;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Agent.Standard
{
    /// <summary>
    /// Helper for converting DTO entities (<see cref="AstEntity"/>, <see cref="AstMethod"/>) 
    /// and also <see cref="CoverageDispatcher"/> from <see cref="InjectedType"/>
    /// </summary>
    public class TreeConverter
    {
        #region AstEntities
        /// <summary>
        /// Convert list of <see cref="InjectedType"/> to the list of <see cref="AstEntity"/>
        /// for transferring onto Admin side
        /// </summary>
        /// <param name="injTypes"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Convert <see cref="InjectedType"/> to the <see cref="AstEntity"/>
        /// for transferring onto Admin side
        /// </summary>
        /// <param name="injType"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Convert <see cref="InjectedMethod"/> to the <see cref="AstMethod"/>
        /// for transferring onto Admin side
        /// </summary>
        /// <param name="injMethod"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Create <see cref="CoverageDispatcher"/> for session <see cref="StartSessionPayload"/> 
        /// and bind it to list of <see cref="InjectedType"/>
        /// </summary>
        /// <param name="session"></param>
        /// <param name="injTypes"></param>
        /// <returns></returns>
        public CoverageDispatcher CreateCoverageDispatcher(StartSessionPayload session, IEnumerable<InjectedType> injTypes)
        {
            //TODO: cloning from Template object?
            var disp = new CoverageDispatcher(session); 
            string testName = session?.TestName ?? $"{AgentConstants.TEST_NAME_DEFAULT}_{Guid.NewGuid()}";
            if(session != null)
                session.TestName = testName;
            var bizTypes = injTypes.Where(a => !a.IsCompilerGenerated)
                .Distinct(new InjectedEntityComparer<InjectedType>());

            var cgMethods = injTypes.Where(a => a.IsCompilerGenerated)
                .SelectMany(a => a.GetMethods().Where(b => b.Points.Any()))
                .Distinct(new InjectedEntityComparer<InjectedMethod>())
                .ToDictionary(k => k.FullName);

            //var cgMethodsTest = injTypes.Where(a => a.IsCompilerGenerated)
            //    .SelectMany(a => a.GetMethods().Where(b => b.Points.Any()))
            //    .OrderBy(a => a.FullName);

            foreach (var type in bizTypes) //don't parallelize yet (need protect ind)
            {
                var bizMethods = type.GetMethods()?
                    .Where(a => a.Coverage.PointToBlockEnds.Any())?
                    .ToList();
                if (bizMethods?.Any() != true) 
                    continue;
                var ind = 0; //end2end for current type
                var execData = new ExecClassData(testName, type.FullName);
                bindMethods(disp, execData, bizMethods, ref ind, cgMethods);
                execData.InitProbes(ind); //not needed +1
            }
            return disp;

            //local methods
            static void bindMethods(CoverageDispatcher disp, ExecClassData execData, List<InjectedMethod> methods, 
                ref int ind, Dictionary<string, InjectedMethod> cgMethods)
            {
                foreach (var meth in methods) //don't parallel here!
                {
                    //own data
                    bindMethod(disp, execData, meth, ref ind);

                    //CG callee data
                    var cgCallees = meth.CalleeIndexes;
                    foreach (var callee in cgCallees.Keys)
                    {
                        var cgInd = cgCallees[callee];
                        if (!cgMethods.ContainsKey(callee))
                            continue; //it's normal (business methods here are not needed)
                        var cgMeth = cgMethods[callee];
                        bindMethod(disp, execData, cgMeth, ref ind);
                    }
                }
            }

            static void bindMethod(CoverageDispatcher disp, ExecClassData execData, InjectedMethod meth, ref int ind)
            {
                var inds = meth.Coverage.PointToBlockEnds.OrderBy(a => a.Value);
                foreach (var pair in inds)
                {
                    var localEnd = pair.Value;
                    var start = ind;
                    var end = start + localEnd;
                    disp.BindPoint(pair.Key, execData, start, end);
                    ind += localEnd + 1;
                }
            }
        }
        #endregion
    }
}