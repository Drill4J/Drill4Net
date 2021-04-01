using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Drill4Net.Injector.Core;
using Drill4Net.Agent.Testing;
using System.Diagnostics.CodeAnalysis;

namespace Drill4Net.Target.NetCore.Tests
{
    //Despite the late binding, a reference to the injected target assembly
    //must be added to the project (as file), which in turn must be updated
    //externally each time the injection process is started (automatically)

    [TestFixture]
    public partial class InjectTargetTests
    {
        private IInjectorRepository _rep;
        private MainOptions _opts;
        private object _targetCommon;

        private Dictionary<string, InjectedSimpleEntity> _pointMap;
        private Dictionary<InjectedSimpleEntity, InjectedSimpleEntity> _parentMap;
        private InjectedSolution _tree;

        /****************************************************************************/

        [OneTimeSetUp]
        public void SetupClass()
        {
            var dirName = InjectorRepository.GetExecutionDir();
            var cfg_path = Path.Combine(dirName, CoreConstants.CONFIG_TESTS_NAME);
            _rep = new InjectorRepository(cfg_path);
            _opts = _rep.Options;

            //this is done on the post-build event of the Injector project
            //var injector = new InjectorEngine(_rep);
            //injector.Process();

            //target assemblies
            //var asms = _opts.Tests.Assemblies;
            _targetCommon = SourceData_Common.TargetCommon; // LoadTargetIntoMemory(asms, targetDir, TestConstants.ASSEMBLY_COMMON);
            //LoadTargetIntoMemory(asms, targetDir, TestConstants.ASSEMBLY_NET5);

            //tree info for targerPath
            var baseDir = _opts.Tests.Directory;
            if (baseDir.EndsWith("\\"))
                baseDir = baseDir.Remove(baseDir.Length - 1, 1);
            var targetDir = $"{baseDir}.{_opts.Destination.FolderPostfix}";
            LoadTreeData(targetDir);
        }

        /****************************************************************************/

        [TestCaseSource(typeof(SourceData_Common), "Simple")]
        public void Simple_Ok(object target, MethodInfo mi, object[] args, List<string> checks)
        {
            Assert.NotNull(mi, $"MethodInfo is empty for: {mi}");

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

            var sig = GetFullSignature(mi);
            var source = SourceDataCore.GetSourceFromFullSig(target, sig);
            Assert.True(funcs.ContainsKey(source));
            var links = funcs[source];

            CheckEnterAndLastReturnOrThrow(links);
            RemoveEnterAndLastReturn(links);
            Check(links, checks);
            #endregion
        }

        [TestCaseSource(typeof(SourceData_Common), "ParentChild")]
        public void Parent_Child_Ok(object target, object[] args, bool isAsync, bool isBunch, bool ignoreEnterReturns, params TestInfo[] inputs)
        {
            #region Arrange
            Assert.IsTrue(inputs?.Length > 0, "Method inputs is empty");
            var parentData = inputs[0];
            var mi = parentData.Info;
            if(string.IsNullOrWhiteSpace(parentData.Signature))
                Assert.NotNull(mi, $"Parent method info is empty");
            #endregion
            #region Act
            try
            {
                if (isAsync)
                {
                    var task = mi.Invoke(target, args) as Task;
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
                        SourceDataCore.GetSourceFromFullSig(target, GetFullSignature(data.Info)) :
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
        private object LoadTargetIntoMemory([NotNull]Dictionary<string, List<string>> assemblies, [NotNull] string targetDir,
            [NotNull] string assemblyName, [NotNull] string className = TestConstants.CLASS_DEFAULT)
        {
            if (!assemblies.ContainsKey(assemblyName))
                throw new ArgumentException($"Assembly [{assemblyName}] not found in config data");
            var asmData = assemblies[assemblyName];
            var classFullName = asmData.FirstOrDefault(a => a.EndsWith($".{className}", StringComparison.InvariantCultureIgnoreCase));
            if (classFullName == null)
                throw new ArgumentException($"Class [{className}] not found for assembly [{assemblyName}] in config");
            //
            var targerPath = Path.Combine(targetDir, assemblyName);
            if (!File.Exists(targerPath))
                Assert.Fail($"Path for target not found: {targerPath}. Check {CoreConstants.CONFIG_TESTS_NAME}");
            var asm = Assembly.LoadFrom(targerPath);
            var type = asm.GetType(classFullName);
            return Activator.CreateInstance(type);
        }

        private void LoadTreeData(string targetDir)
        {
            //search dir with files - there must be tree data
            var curDir = targetDir;
            while (true)
            {
                if (Directory.GetFiles(curDir, "*.dll").Length > 0)
                    break;
                var dirs = Directory.GetDirectories(curDir);
                if (dirs.Length == 0)
                    Assert.Fail($"Tree info not found in {targetDir}");
                curDir = dirs[0];
            }
            
            //read tree data
            var treeHintPath = _rep.GetTreeFileHintPath(curDir);
            if (!File.Exists(treeHintPath))
                Assert.Fail($"File with hint about tree data not found: {treeHintPath}");
            var treePath = File.ReadAllText(treeHintPath);
            _tree = _rep.ReadInjectedTree(treePath);
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

        internal static string GetFullSignature(MethodInfo mi)
        {
            var func = mi.Name;
            var type = mi.DeclaringType;
            var className = $"{type.Namespace}.{type.Name}";

            //parameters
            var pars = mi.GetParameters();
            var parNames = string.Empty;
            var lastInd = pars.Length - 1;
            for (var j = 0; j <= lastInd; j++)
            {
                var p = pars[j].ParameterType;
                var pName = p.FullName;
                if (pName.Contains("Version=")) //need simplify strong named type
                {
                    pName = $"{p.Namespace}.{p.Name}<{string.Join(",", p.GenericTypeArguments.Select(a => a.FullName))}>";
                }
                parNames += pName;
                if (j < lastInd)
                    parNames += ",";
            }

            //return type
            var retType = mi.ReturnType.FullName;
            if (retType.Contains("Version=")) //need simplify strong named type
            {
                retType = mi.ReturnParameter.ToString()
                    .Replace("[", "<").Replace("]", ">").Replace(" ", null);
            }
            var sig = $"{retType} {className}::{func}({parNames})";

            return sig;
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