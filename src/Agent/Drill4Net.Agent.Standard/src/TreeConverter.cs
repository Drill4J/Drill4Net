using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Drill4Net.Profiling.Tree;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Standard
{
    /// <summary>
    /// Helper for converting DTO entities (<see cref="AstEntity"/>, <see cref="AstMethod"/>) 
    /// and also <see cref="CoverageRegistrator"/> from <see cref="InjectedType"/>
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
            var res = new ConcurrentBag<AstEntity>();
            foreach (var type in injTypes.Where(a => !a.IsCompilerGenerated).AsParallel())
            {
                lock (res)
                    res.Add(ToAstEntity(type));
            }
            return res.ToList();
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
            var injMethods = GetOrderedBusinessMethods(injType);
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
            var sig = injMethod.Signature;
            var astMethod = new AstMethod(injMethod.Name, sig.Return, injMethod.BusinessSize, injMethod.Source.HashCode);
            if (sig.Parameters != null)
                astMethod.@params = sig.Parameters.Split(',').Select(a => a.Trim()).ToList();
            return astMethod;
        }
        #endregion
        #region ExecClassData
        /// <summary>
        /// Create <see cref="CoverageRegistrator"/> for session <see cref="StartSessionPayload"/> 
        /// and bind it to list of <see cref="InjectedType"/>
        /// </summary>
        /// <param name="session"></param>
        /// <param name="injTypes"></param>
        /// <returns></returns>
        public CoverageRegistrator CreateCoverageRegistrator(StartSessionPayload session, IEnumerable<InjectedType> injTypes)
        {
            //TODO: cloning from some Template object?
            var reg = new CoverageRegistrator(session);
            var testName = session?.TestName ?? Guid.NewGuid().ToString();
            if(session != null)
                session.TestName = testName;
            var bizTypes = injTypes
                .Where(a => !a.IsCompilerGenerated)
                .Distinct(new InjectedEntityComparer<InjectedType>());

            var cgMethods = injTypes
                .SelectMany(a => a.Filter(typeof(InjectedMethod), true)) //including nested types/methods
                .Cast<InjectedMethod>()
                .Where(a => a.IsCompilerGenerated && a.Points.Any())
                .Distinct(new InjectedEntityComparer<InjectedMethod>())
                .ToDictionary(k => k.FullName);

            foreach (var type in bizTypes) //don't parallelize (need protect ind)
            {
                var bizMethods = GetOrderedBusinessMethods(type);
                if (bizMethods?.Any() != true)
                    continue;
                var ind = 0; //end2end for the current type
                var execData = new ExecClassData(testName, type.FullName);
                BindMethods(reg, execData, bizMethods, ref ind, cgMethods);

                var cnt = bizMethods.Sum(a => a.BusinessSize);
                execData.InitProbes(cnt);
            }
            return reg;
        }

        /// <summary>
        /// Binds the methods to the class by start and end indexes to the class coverage's array
        /// </summary>
        /// <param name="reg"></param>
        /// <param name="execData"></param>
        /// <param name="methods"></param>
        /// <param name="ind"></param>
        /// <param name="cgMethods"></param>
        internal void BindMethods(CoverageRegistrator reg, ExecClassData execData, IEnumerable<InjectedMethod> methods,
            ref int ind, Dictionary<string, InjectedMethod> cgMethods)
        {
            foreach (var meth in methods) //don't parallel here!
            {
                //own data
                BindMethod(reg, execData, meth.Structure, ref ind);

                //callee's data
                var callees = meth.CalleeOrigIndexes;
                foreach (var callee in callees.Keys)
                {
                    //here we need only compiler generated methods
                    if (!cgMethods.ContainsKey(callee))
                        continue;
                    var cgMeth = cgMethods[callee];
                    BindMethod(reg, execData, cgMeth.Structure, ref ind);
                }
            }
        }

        /// <summary>
        /// Binds the method to the class by start and end indexes to the class coverage's array
        /// </summary>
        /// <param name="reg"></param>
        /// <param name="execData"></param>
        /// <param name="methStruct"></param>
        /// <param name="ind"></param>
        internal void BindMethod(CoverageRegistrator reg, ExecClassData execData, MethodStructure methStruct, ref int ind)
        {
            var indPairs = methStruct.PointToBlockEnds.OrderBy(a => a.Value);
            var startMeth = ind;
            foreach (var pair in indPairs) //don't parallel here!
            {
                var localEnd = pair.Value;
                var startBlock = ind;
                var endBlock = startMeth + localEnd;
                reg.BindPoint(pair.Key, execData, startBlock, endBlock);
                ind = endBlock + 1;
            }
        }
        #endregion

        internal IOrderedEnumerable<InjectedMethod> GetOrderedBusinessMethods(InjectedType injType)
        {
            var injMethods = injType?.GetMethods()?
                .Where(a => !a.IsCompilerGenerated && a.Structure.PointToBlockEnds.Any())
                .OrderBy(a => a, new MethodNameComparer());
            return injMethods;
        }
    }
}