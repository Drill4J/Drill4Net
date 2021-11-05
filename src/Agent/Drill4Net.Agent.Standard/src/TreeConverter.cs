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
            return res.OrderBy(a => a.name).ToList();
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
        /// <param name="context"></param>
        /// <param name="session"></param>
        /// <param name="injTypes"></param>
        /// <returns></returns>
        public CoverageRegistrator CreateCoverageRegistrator(string context, StartSessionPayload session, IEnumerable<InjectedType> injTypes)
        {
            //TODO: cloning from some Template object !!!
            var reg = new CoverageRegistrator(context, session);
            var testName = session?.TestName ?? $"OutOfTest_{Guid.NewGuid()}";
            if (session != null)
                session.TestName = testName;
            var bizTypes = injTypes
                .Where(a => !a.IsCompilerGenerated)
                .Distinct(new InjectedEntityComparer<InjectedType>());

            foreach (var type in bizTypes) //don't parallelize (need protect ind)
            {
                var bizMethods = GetOrderedBusinessMethods(type);
                if (bizMethods?.Any() != true)
                    continue;
                
                //calculating
                var typeName = type.FullName;
                var execData = new ExecClassData(testName, typeName);
                var cnt = BindMethods(reg, execData, bizMethods);

                //checking
                var size = bizMethods.Sum(a => a.BusinessSize);
                if (cnt != size)
                    throw new Exception($"Error in data: actual and calculated on indexes sizes of type {typeName} are not matched");

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
        /// <returns>Count of instructions</returns>
        internal int BindMethods(CoverageRegistrator reg, ExecClassData execData, IEnumerable<InjectedMethod> methods)
        {
            var end = 0;
            foreach (var meth in methods) //don't parallel here!
            {
                var prevInd = -1;
                foreach (var (ind, uid) in meth.End2EndBusinessIndexes)
                {
                    var start = end;
                    end += ind - prevInd - 1;
                    reg.BindPoint(uid, execData, start, end);
                    prevInd = ind;
                    end++;
                }
            }
            return end;
        }
        #endregion

        internal IOrderedEnumerable<InjectedMethod> GetOrderedBusinessMethods(InjectedType injType)
        {
            var injMethods = injType?.GetMethods()?
                .Where(a => !a.IsCompilerGenerated && a.End2EndBusinessIndexes.Count > 0)
                .OrderBy(a => a, new MethodNameComparer());
            return injMethods;
        }
    }
}