using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Testing;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Target.Tests.Engine
{
    // TODO: move the Engine to own assembly!

    //Despite the late binding, a reference to the injected target assembly
    //must be added to the project (as file), which in turn must be updated
    //externally each time the injection process is started (automatically)

    /// <summary>
    /// Tests for the injected Target assembly
    /// </summary>
    [TestFixture]
    public abstract class AbstractTargetTestEngine
    {
        protected static readonly TestEngineRepository _testsRep;
        private static Dictionary<string, CrossPoint> _pointMap;
        private static Dictionary<InjectedSimpleEntity, InjectedSimpleEntity> _parentMap;
        private static InjectedSolution _tree;
        private static readonly Logger _logger;

        /****************************************************************************/

        static AbstractTargetTestEngine()
        {
            try
            {
                _testsRep = new TestEngineRepository();
                _logger = new TypedLogger<AbstractTargetTestEngine>(_testsRep.Subsystem);

                LoadTreeData();
                _logger.Info("Engine is initialized.");
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex.ToString());
                throw;
            }
        }

        /****************************************************************************/

        /// <summary>
        /// Target tests which affect only one method
        /// </summary>
        [TestCaseSource(typeof(CommonSourceData), nameof(CommonSourceData.Simple))]
        public void Base_Simple(MethodInfo mi, object[] args, List<string> checks)
        {
            Assert.NotNull(mi, "Method info is empty");

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
            catch { } //it's normal for business throws
            #endregion
            #region Assert
            var funcs = GetFunctions();
            _testsRep.WriteDebugInfo(mi.Name, args, funcs);
            Assert.IsTrue(funcs.Count == 1, $"Must be 1 function. Real: {funcs.Count}");

            var sig = SourceDataCore.GetFullSignature(mi);
            var source = SourceDataCore.GetSourceFromFullSig(target, sig);
            Assert.True(funcs.ContainsKey(source));
            var links = funcs[source];

            CheckLastReturnOrEnterOrThrow(links);
            RemoveEnterAndLastReturn(links);
            Check(links, checks);
            #endregion
        }

        /// <summary>
        /// Target tests which affect several methods
        /// </summary>
        [TestCaseSource(typeof(CommonSourceData), nameof(CommonSourceData.Parented))]
        public void Base_Parented(MethodInfo mi, object[] args, bool isAsync, bool isBunch, bool ignoreEnterReturns, params TestMetadata[] inputs)
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
                    Assert.IsNotNull(mi);
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
                var probes2 = TestAgent.GetPointsIgnoringContext(sig);
                var links2 = ConvertToLinks(probes2);
                funcs.Add(sig, links2);
            }
            _testsRep.WriteDebugInfo(mi.Name, args, funcs);

            if (!isBunch)
            {
                Assert.IsTrue(funcs.Count == inputs.Length, $"Must be {inputs.Length} function(s). Real: {funcs.Count}");

                //by subfunctions in main calls
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
                        CheckLastReturnOrEnterOrThrow(points);
                        RemoveEnterAndLastReturn(points);
                    }

                    var checks = data.Checks;
                    if (data.NeedSort)
                    {
                        points.Sort(new PointLinkageProbeComparer());
                        checks.Sort();
                    }
                    Check(points, checks);
                }
            }
            else
            {
                var checks = inputs.SelectMany(a => a.Checks).ToList();
                var points = funcs.Values.SelectMany(a => a).ToList();
                RemoveEnterReturns(points);
                if (inputs.Any( a=> a.NeedSort))
                {
                    points.Sort();
                    checks.Sort();
                }
                Check(points, checks);
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
            _logger.Debug("Tree data is loading...");

            _tree = _testsRep.LoadTree();
            _parentMap = _tree.CalcParentMap();
            _pointMap = _tree.MapPoints(_parentMap);

            _logger.Debug("Tree data is loaded.");
        }

        private void CheckLastReturnOrEnterOrThrow(List<PointLinkage> links)
        {
            var probes = links.Select(a => a.Probe);
            Assert.IsNotNull(probes);
            if(!_testsRep.Options.Probes.SkipEnterType)
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
            var raw = TestAgent.GetMethods(false);
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
            if (probData == null)
                throw new ArgumentException(nameof(probData));
            //
            string uid = probData;
            if (probData.Contains(':'))
            {
                var ar = probData.Split(':');
                uid = ar[0];
            }
            if (!_pointMap.ContainsKey(uid))
                Assert.Fail($"No point with Uid = {uid}");
            //
            var point = _pointMap[uid];
            var method = (InjectedMethod)_parentMap[point];
            var type = (InjectedType)_parentMap[method];

            InjectedSimpleEntity asmObj = type;
            do { asmObj = _parentMap[asmObj]; } while (asmObj is { } and not InjectedAssembly);
            var asm = asmObj as InjectedAssembly;

            return new PointLinkage(asm, type, method, point);
        }
        
        private static void Check(IList<PointLinkage> reals, List<string> checks)
        {
            //Assert.That(links.Select(a => a.Probe), Is.EqualTo(checks));
            var checkInd = -1;
            var lastCheckInd = checks.Count - 1;
            for (int i = 0; i < reals.Count; i++)
            {
                checkInd++;
                var real = reals[i].Probe;
                if (lastCheckInd < checkInd)
                    Assert.Fail($"Index: {i}");

                var must = checks[checkInd];
                if (must.Contains('|'))
                {
                    var ar = must.Split('|'); //several variants of the probe
                    var empty = ar.FirstOrDefault(a => string.IsNullOrWhiteSpace(a)); //the probe can be missed
                    ar = ar.Where(a => !string.IsNullOrWhiteSpace(a)).ToArray();
                    var yes = false;
                    foreach (var check in ar)
                    {
                        if (check != real)
                            continue;
                        yes = true;
                        break;
                    }
                    if (yes)
                        continue;
                    if (empty != null)
                    {
                        i--;
                        continue;
                    }
                    //
                    Assert.Fail($"Index: {i}");
                }
                else
                {
                    Assert.AreEqual(must, real, $"Index: {i}");
                }
            }
        }
        #endregion
    }
}