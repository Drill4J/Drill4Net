using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Drill4Net.Injector.Core;
using Drill4Net.Plugins.Testing;

namespace Drill4Net.Target.Comon.Tests
{
    //Despite the late binding, a reference to the injected target assembly
    //must be added to the project (as file), which in turn must be updated
    //externally each time the injection process is started (automatically)

    [TestFixture]
    public partial class InjectTargetTests
    {
        private IInjectorRepository _rep;
        private MainOptions _opts;
        private Type _type;
        private object _target;

        /****************************************************************************/

        [OneTimeSetUp]
        public void SetupClass()
        {
            var cfg_path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), CoreConstants.CONFIG_TESTS_NAME);
            _rep = new InjectorRepository(cfg_path);
            //this is done on the post-build event of the Injector project
            //var injector = new InjectorEngine(_rep);
            //injector.Process();

            var _opts = _rep.Options;
            var profPath = Path.Combine(_opts.Destination.Directory, _opts.Tests.AssemblyName);
            var asm = Assembly.LoadFrom(profPath);
            _type = asm.GetType($"{_opts.Tests.Namespace}.{_opts.Tests.Class}");
            _target = Activator.CreateInstance(_type);
        }

        /****************************************************************************/

        [TestCaseSource(typeof(SourceData), "Simple"/*, Category = "Simple"*/)]
        public void Simple_Ok(MethodInfo mi, object[] args, List<string> checks)
        {
            //arrange
            Assert.NotNull(mi, $"MethodInfo is empty for: {mi}");

            //act
            try
            {
                mi.Invoke(_target, args);
            }
            catch { } //it's normal for business throws

            //assert
            var funcs = GetFunctions();
            Assert.IsTrue(funcs.Count == 1);

            var sig = GetFullSignature(mi);
            var source = GetSource(sig);
            Assert.True(funcs.ContainsKey(source));
            var points = funcs[source];

            CheckEnterAndLastReturnOrThrow(points);
            RemoveEnterAndLastReturn(points);
            Check(points, checks);
        }

        [TestCaseSource(typeof(SourceData), "ParentChild", Category = "ParentChild")]
        public void Parent_Child_Ok(object[] args, bool isAsync, bool ignoreEnterReturns, params TestData[] inputs)
        {
            //arrange
            var parentData = inputs[0];
            var mi = parentData.Info;
            Assert.NotNull(mi, $"MethodInfo is empty for: {mi}");

            //act
            if (isAsync)
            {
                var task = mi.Invoke(_target, args) as Task;
                task.Wait();
            }
            else
            {
                mi.Invoke(_target, args);
            }

            //assert
            var funcs = GetFunctions();
            Assert.IsTrue(funcs.Count == inputs.Length);

            for (var i = 0; i < inputs.Length; i++)
            {
                var data = inputs[i];
                var source = GetSource(GetFullSignature(data.Info));
                Assert.True(funcs.ContainsKey(source));
                var points = funcs[source];

                if (ignoreEnterReturns)
                {
                    var forDelete = points.Where(a => a.StartsWith("Enter_") || a.StartsWith("Return_")).ToArray();
                    for (var j = 0; j < forDelete.Length; j++)
                        points.Remove(forDelete[j]);
                }
                else
                {
                    CheckEnterAndLastReturnOrThrow(points);
                    RemoveEnterAndLastReturn(points);
                }

                if (data.NeedSort)
                    points.Sort();
                Check(points, data.Checks);
            }
        }

        private void RemoveEnterAndLastReturn(List<string> points)
        {
            //Check() not checks Enter and last Return
            if (points[0].StartsWith("Enter_"))
                points.RemoveAt(0);
            var lastInd = points.Count - 1;
            if (points[lastInd].StartsWith("Return_"))
                points.RemoveAt(lastInd);
        }

        #region Auxiliary funcs
        private List<string> GetPoints(string shortSig)
        {
            var funcSig = GetFullSignature(shortSig);
            var asmName = GetModuleName();
            return TestProfiler.GetPoints(asmName, funcSig, false);
        }

        private string GetSource(string sig)
        {
            var funcSig = sig;
            var asmName = GetModuleName();
            return $"{asmName};{funcSig}";
        }

        private string GetModuleName()
        {
            return "Drill4Net.Target.Common.dll";
        }

        private string GetFullSignature(string shortSig)
        {
            var ar = shortSig.Split(' ');
            var ret = ar[0];
            var name = ar[1];
            return $"{ret} Drill4Net.Target.Common.InjectTarget::{name}";
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
                var p = pars[j];
                parNames += p.ParameterType.FullName;
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

        private Dictionary<string, List<string>> GetFunctions()
        {
            return TestProfiler.GetFunctions(false);
        }

        private MethodInfo GetMethod(string shortSig)
        {
            var name = GetNameFromSig(shortSig);
            return _type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private string GetNameFromSig(string shortSig)
        {
            var name = shortSig.Split(' ')[1];
            name = name.Substring(0, name.IndexOf("("));
            return name;
        }

        private void CheckEnterAndLastReturnOrThrow(List<string> points)
        {
            Assert.IsNotNull(points.FirstOrDefault(a => a == "Enter_0"), "No Enter");
            Assert.IsNotNull(points.Last(a => a.StartsWith("Return_") || a.StartsWith("Throw_")), "No last Return/Throw");
        }

        private void Check(List<string> points, List<string> checks)
        {
            Assert.IsTrue(points.Count == checks.Count);
            for (var i = 0; i < checks.Count; i++)
            {
                if (points[i] != checks[i])
                    Assert.Fail();
            }
        }
        #endregion
    }
}