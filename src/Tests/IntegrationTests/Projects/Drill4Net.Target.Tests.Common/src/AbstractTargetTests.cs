using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Drill4Net.Agent.Testing;
using Drill4Net.Profiling.Tree;
using Serilog;

namespace Drill4Net.Target.Tests.Common
{
    //Despite the late binding, a reference to the injected target assembly
    //must be added to the project (as file), which in turn must be updated
    //externally each time the injection process is started (automatically)

    [TestFixture]
    public abstract class AbstractTargetTests
    {
        protected static readonly TestEngineRepository _testsRep;
        private static Dictionary<string, InjectedSimpleEntity> _pointMap;
        private static Dictionary<InjectedSimpleEntity, InjectedSimpleEntity> _parentMap;
        private static InjectedSolution _tree;
        //private Dictionary<string, object> _types;

        /****************************************************************************/

        static AbstractTargetTests()
        {
            _testsRep = new TestEngineRepository();
            LoadTreeData();
            Log.Information("Engine is initialized.");
        }

        /****************************************************************************/

        [OneTimeSetUp]
        public void SetupClass()
        {
            //LoadTarget();
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

        [TestCaseSource(typeof(CommonSourceData), nameof(CommonSourceData.Simple))]
        public void Base_Simple(MethodInfo mi, object[] args, List<string> checks)
        {
            Assert.NotNull(mi, $"Method info is empty");

            //arrange
            var target = CommonSourceData.Target;

            #region Act
            try
            {
                mi.Invoke(target, args);
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
            var source = SourceDataCore.GetSourceFromFullSig(target, sig);
            Assert.True(funcs.ContainsKey(source));
            var links = funcs[source];

            CheckEnterAndLastReturnOrThrow(links);
            RemoveEnterAndLastReturn(links);
            Check(links, checks);
            #endregion
        }

        [TestCaseSource(typeof(CommonSourceData), nameof(CommonSourceData.Parented))]
        public void Base_Parented(MethodInfo mi, object[] args, bool isAsync, bool isBunch, bool ignoreEnterReturns, params TestInfo[] inputs)
        {
            #region Arrange
            Assert.IsTrue(inputs?.Length > 0, "Method inputs is empty");

            var target = CommonSourceData.Target;
            var parentData = inputs[0];
            #endregion
            #region Act
            try
            {
                if (isAsync)
                {
                    var task = mi.Invoke(target, args) as Task;
                    if(task == null)
                        Assert.Fail("Task for calling is empty");
                    task.Wait();
                }
                else
                {
                    mi.Invoke(target, args);
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
                if (!inputs.SelectMany(a => a.Checks).Any(a => a.Contains("Throw")))
                    Assert.Fail(ex.Message);
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
                var probes2 = TestingProfiler.GetPointsIgnoringContext(sig);
                var links2 = ConvertToLinks(probes2);
                funcs.Add(sig, links2);
            }
            if (!isBunch)
            {
                Assert.IsTrue(funcs.Count == inputs.Length);

                foreach (var data in inputs)
                {
                    string source;
                    string fullSig = null;
                    if (!string.IsNullOrWhiteSpace(data.Signature))
                    {
                        source = data.Signature;
                    }
                    else
                    {
                        //for child methods exactly data.Info, not for current mi
                        fullSig = SourceDataCore.GetFullSignature(data.Info);
                        source = SourceDataCore.GetSourceFromFullSig(target, fullSig);
                    }

                    List<PointLinkage> points = null;
                    if (!funcs.ContainsKey(source)) //rare: another source?
                    {
                        //var asmName = SourceDataCore.CreateMethodSource(,fullSig);
                        foreach (var func in funcs.Keys)
                        {
                            var ar = func.Split(';');
                            if (ar[1] == fullSig)
                            {
                                points = funcs[func];
                                break;
                            }
                        }
                        Assert.NotNull(points);
                    }
                    else
                    {
                        points = funcs[source];
                    }

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
            Log.Debug("Tree data is loading...");

            _tree = _testsRep.LoadTree();
            _parentMap = _tree.CalcParentMap();
            _pointMap = _tree.CalcPointMap(_parentMap);

            Log.Debug("Tree data is loaded.");
        }

        private void CheckEnterAndLastReturnOrThrow(List<PointLinkage> links)
        {
            var probes = links.Select(a => a.Probe);
            Assert.IsNotNull(probes);
            Assert.IsNotNull(probes.FirstOrDefault(a => a == "Enter_0"), "No Enter");
            Assert.IsNotNull(probes.Last(a => a.StartsWith("Return_") || a.StartsWith("Throw_")), "No last Return/Throw");
        }

        private void RemoveEnterAndLastReturn(List<PointLinkage> links)
        {
            //because further Check() not checks Enter and last Return
            if (links[0].Point.PointType == CrossPointType.Enter)
                links.RemoveAt(0);
            var lastInd = links.Count - 1;
            if (links[lastInd].Point.PointType == CrossPointType.Return)
                links.RemoveAt(lastInd);
        }

        private Dictionary<string, List<PointLinkage>> GetFunctions()
        {
            var raw = TestingProfiler.GetFunctions(false);
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
            if (probData == null || probData.Contains(':') == false)
                throw new ArgumentException(nameof(probData));
            //
            var ar = probData.Split(':');
            var uid = ar[0];
            if (!_pointMap.ContainsKey(uid))
                Assert.Fail($"No point with Uid = {uid}");
            //
            var point = (CrossPoint)_pointMap[uid];
            var method = (InjectedMethod)_parentMap[point];
            var type = (InjectedType)_parentMap[method];

            InjectedSimpleEntity asmObj = type;
            do { asmObj = _parentMap[asmObj]; }
            //asmObj != null && asmObj is not InjectedAssembly
            while (asmObj is { } and not InjectedAssembly); 
            var asm = asmObj as InjectedAssembly;
            //
            var link = new PointLinkage(asm, type, method, point);
            return link;
        }
        
        private static void Check(IList<PointLinkage> links, List<string> checks)
        {
            Assert.That(links.Select(a => a.Probe), Is.EqualTo(checks));
        }
        #endregion
    }
}