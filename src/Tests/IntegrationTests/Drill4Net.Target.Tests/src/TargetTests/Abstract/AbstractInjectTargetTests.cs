using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using Drill4Net.Injector.Core;
using Drill4Net.Agent.Testing;

namespace Drill4Net.Target.Tests
{
    //Despite the late binding, a reference to the injected target assembly
    //must be added to the project (as file), which in turn must be updated
    //externally each time the injection process is started (automatically)

    [TestFixture]
    [NonParallelizable]
    internal abstract class AbstractInjectTargetTests
    {
        protected static TestEngineRepository _testsRep;
        private static Dictionary<string, InjectedSimpleEntity> _pointMap;
        private static Dictionary<InjectedSimpleEntity, InjectedSimpleEntity> _parentMap;
        private static InjectedSolution _tree;
        private Dictionary<string, object> _types;
        protected object _target;

        /****************************************************************************/

        static AbstractInjectTargetTests()
        {
            _testsRep = new TestEngineRepository();
            LoadTreeData();
        }

        /****************************************************************************/

        [OneTimeSetUp]
        public void SetupClass()
        {
            _types = LoadTarget();
            SetTarget();
        }

        [OneTimeTearDown]
        public void TearDownClass()
        {
            //it seems that unloading assemblies from memory technically works,
            //but the tests although with the correct statistics
            //do not look very nice in NUnit

            //UnloadTarget();
        }

        /****************************************************************************/

        protected abstract Dictionary<string, object> LoadTarget();
        protected abstract void UnloadTarget();

        protected virtual void SetTarget()
        {
            _target = _types[TestConstants.CLASS_DEFAULT_FULL];
        }

        [TestCaseSource(typeof(SourceData_Common), "Simple")]
        public void Base_Simple(string methodName, object[] args, List<string> checks)
        {
            Assert.NotNull(methodName, $"Method name is empty");

            //arrange
            var mi = _target.GetType().GetMethod(methodName);
            if (mi == null)
                Assert.Fail($"Methof [{methodName}] not found");

            #region Act
            try
            {
                mi.Invoke(_target, args);
            }
            catch (FileNotFoundException fex)
            {
                Assert.Fail(fex.Message);
            }
            catch (TargetException tex)
            {
                Assert.Fail(tex.Message);
            }
            catch (Exception ex) //it's normal for business throws
            {
                if (!checks.Any(a => a.Contains("Throw")))
                    Assert.Fail(ex.Message);
            }
            #endregion
            #region Assert
            var funcs = GetFunctions();
            Assert.IsTrue(funcs.Count == 1);

            var sig = SourceDataCore.GetFullSignature(mi);
            var source = SourceDataCore.GetSourceFromFullSig(_target, sig);
            Assert.True(funcs.ContainsKey(source));
            var links = funcs[source];

            CheckEnterAndLastReturnOrThrow(links);
            RemoveEnterAndLastReturn(links);
            Check(links, checks);
            #endregion
        }

        [TestCaseSource(typeof(SourceData_Common), "Parented")]
        public void Base_Parented(object[] args, bool isAsync, bool isBunch, bool ignoreEnterReturns, params TestInfo[] inputs)
        {
            #region Arrange
            Assert.IsTrue(inputs?.Length > 0, "Method inputs is empty");
            var parentData = inputs[0];
            var mName = parentData.Info.Name;
            var mi = _target.GetType().GetMethod(mName); //need re-create for current real type

            if (string.IsNullOrWhiteSpace(parentData.Signature))
                Assert.NotNull(mi, $"Method [{mName}] not found");
            #endregion
            #region Act
            try
            {
                if (isAsync)
                {
                    var task = mi.Invoke(_target, args) as Task;
                    task.Wait();
                }
                else
                {
                    mi.Invoke(_target, args);
                }
            }
            catch (FileNotFoundException fex)
            {
                Assert.Fail(fex.Message);
            }
            catch (TargetException tex) 
            {
                Assert.Fail(tex.Message);
            }
            catch (Exception ex) //it's normal for business throws
            {
                //if (!inputs.Select(a => a.Checks).Any(a => a.Contains("Throw")))
                    //Assert.Fail(ex.Message);
            }
            #endregion
            #region Assert
            var funcs = GetFunctions();

            //checking whether functions from another context are needed
            foreach (var input in inputs.Where(a => a.IgnoreContextForSig))
            {
                var sig = input.Signature;
                if (funcs.ContainsKey(sig))
                    continue;
                var probes2 = TesterProfiler.GetPointsIgnoringContext(sig);
                var links2 = ConvertToLinks(probes2);
                funcs.Add(sig, links2);
            }
            if (!isBunch)
            {
                Assert.IsTrue(funcs.Count == inputs.Length);

                for (var i = 0; i < inputs.Length; i++)
                {
                    var data = inputs[i];
                    var source = string.IsNullOrWhiteSpace(data.Signature) ?
                        SourceDataCore.GetSourceFromFullSig(_target, SourceDataCore.GetFullSignature(data.Info)) : //for child methods exactly data.Info, not for current mi
                        data.Signature;
                    Assert.True(funcs.ContainsKey(source));
                    var points = funcs[source];

                    if (ignoreEnterReturns)
                    {
                        RemoveEnterReturns(points);
                    }
                    else
                    {
                        CheckEnterAndLastReturnOrThrow(points);
                        RemoveEnterAndLastReturn(points);
                    }

                    if (data.NeedSort)
                        points.Sort(new PointLinkageProbeComparer());
                    Check(points, data.Checks);
                }
            }
            else
            {
                var data = inputs[0];
                var points = funcs.Values.SelectMany(a => a).ToList();
                RemoveEnterReturns(points);
                if (data.NeedSort)
                    points.Sort();
                Check(points, data.Checks);
            }

            //local funcs
            static void RemoveEnterReturns(IList<PointLinkage> links)
            {
                if (links is null)
                    throw new ArgumentNullException(nameof(links));

                var forDelete = links.Where(a => a.Point.PointType == CrossPointType.Enter || 
                                                 a.Point.PointType == CrossPointType.Return)
                    .ToArray();
                for (var j = 0; j < forDelete.Length; j++)
                    links.Remove(forDelete[j]);
            }
            #endregion
        }

        #region Auxiliary funcs
        private static void LoadTreeData()
        {
            _tree = _testsRep.LoadTree();
            _parentMap = _tree.CalcParentMap();
            _pointMap = _tree.CalcPointMap(_parentMap);
        }

        private void CheckEnterAndLastReturnOrThrow(List<PointLinkage> links)
        {
            var probes = links.Select(a => a.Probe);
            Assert.IsNotNull(probes.FirstOrDefault(a => a == "Enter_0"), "No Enter");
            Assert.IsNotNull(probes.Last(a => a.StartsWith("Return_") || a.StartsWith("Throw_")), "No last Return/Throw");
        }

        private void RemoveEnterAndLastReturn(List<PointLinkage> links)
        {
            //Check() not checks Enter and last Return
            if (links[0].Point.PointType == CrossPointType.Enter)
                links.RemoveAt(0);
            var lastInd = links.Count - 1;
            if (links[lastInd].Point.PointType == CrossPointType.Return)
                links.RemoveAt(lastInd);
        }

        private Dictionary<string, List<PointLinkage>> GetFunctions()
        {
            var raw = TesterProfiler.GetFunctions(false);
            return ConvertToLinks(raw);
        }

        private Dictionary<string, List<PointLinkage>> ConvertToLinks(Dictionary<string, List<string>> funcs)
        {
            var res = new Dictionary<string, List<PointLinkage>>();

            //key is func name, val - is in format $"{uid}:{probe}"
            foreach (var func in funcs.Keys)
            {
                var probDatas = funcs[func];
                var links = ConvertToLinks(probDatas);
                res.Add(func, links);
            }
            return res;
        }

        internal List<PointLinkage> ConvertToLinks(List<string> probDatas)
        {
            var links = new List<PointLinkage>();
            foreach (var probData in probDatas)
            {
                var link = ConvertToLink(probData);
                links.Add(link);
            }
            return links;
        }

        internal PointLinkage ConvertToLink(string probData)
        {
            if (probData?.Contains(":") == false)
                throw new ArgumentException(nameof(probData));
            //
            var ar = probData.Split(":");
            var uid = ar[0];
            if (!_pointMap.ContainsKey(uid))
                Assert.Fail($"No point with Uid = {uid}");
            //
            var point = _pointMap[uid] as CrossPoint;
            var method = _parentMap[point] as InjectedMethod;
            var type = _parentMap[method] as InjectedType;

            InjectedSimpleEntity asmObj = type;
            do { asmObj = _parentMap[asmObj]; }
            while (asmObj != null && asmObj is not InjectedAssembly);
            var asm = asmObj as InjectedAssembly;
            //
            var link = new PointLinkage(asm, type, method, point);
            return link;
        }
        
        private static void Check([NotNull]IList<PointLinkage> links, [NotNull]List<string> checks)
        {
            Assert.That(links.Select(a => a.Probe), Is.EqualTo(checks));
        }
        #endregion
    }
}