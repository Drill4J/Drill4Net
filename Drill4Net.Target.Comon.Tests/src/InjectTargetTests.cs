using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Drill4Net.Injector.Core;
using Drill4Net.Plugins.Testing;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Drill4Net.Target.Comon.Tests
{
    //Despite the late binding, a reference to the injected target assembly
    //must be added to the project (as file), which in turn must be updated
    //externally each time the injection process is started (automatically)

    [TestFixture]
    public class InjectTargetTests
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

        [TestCaseSource(typeof(SourceData), "Simple", Category = "Simple")]
        public void Simple_Ok(string func, object[] args, string shortSig, List<string> parentChecks)
        {
            //arrange
            var mi = GetMethod(func);

            //act
            mi.Invoke(_target, args);

            //assert
            var funcs = GetFunctions();
            Assert.IsTrue(funcs.Count == 1);

            var sig = GetSource(shortSig);
            Assert.True(funcs.ContainsKey(sig));
            var parentFunc = funcs[sig];

            CheckEnterAndLastReturn(parentFunc);
            Check(parentFunc, parentChecks);
        }

        [TestCaseSource(typeof(SourceData), "ParentChild", Category = "ParentChild")]
        public void Parent_Child_Ok(string func, object[] args, 
                                    string parentShortSig, List<string> parentChecks, 
                                    string childShortSig, List<string> childChecks)
        {
            //arrange
            var mi = GetMethod(func);

            //act
            mi.Invoke(_target, args);

            //assert
            var funcs = GetFunctions();
            Assert.IsTrue(funcs.Count == 2);

            //1. parent func
            var parentSig = GetSource(parentShortSig);
            Assert.True(funcs.ContainsKey(parentSig));
            var parentFunc = funcs[parentSig];
            CheckEnterAndLastReturn(parentFunc);
            Check(parentFunc, parentChecks);

            //2. child func
            var childSig = GetSource(childShortSig);
            Assert.True(funcs.ContainsKey(childSig));
            var childFunc = funcs[childSig];
            CheckEnterAndLastReturn(childFunc);
            Check(childFunc, childChecks);
        }

        #region Auxiliary funcs
        private List<string> GetPoints(string shortSig)
        {
            var funcSig = GetFullSignature(shortSig);
            var asmName = GetModuleName();
            return TestProfiler.GetPoints(asmName, funcSig, false);
        }

        private string GetSource(string shortSig)
        {
            var funcSig = GetFullSignature(shortSig);
            var asmName = GetModuleName();
            return $"{asmName};{funcSig}";
        }

        private string GetModuleName()
        {
            return "Drill4Net.Target.Common.dll";
        }

        private string GetFullSignature(string shortSig)
        {
            return $"System.Void Drill4Net.Target.Common.InjectTarget::{shortSig}";
        }

        private Dictionary<string, List<string>> GetFunctions()
        {
            return TestProfiler.GetFunctions(false);
        }

        private MethodInfo GetMethod(string name)
        {
            return _type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private void CheckEnterAndLastReturn(List<string> points)
        {
            Assert.IsNotNull(points.FirstOrDefault(a => a == "Enter_0"), "No Enter");
            Assert.IsNotNull(points.Last(a => a.StartsWith("Return_")), "No Return");
        }

        private void Check(List<string> points, List<string> checks)
        {
            Assert.True(points.Count == checks.Count + 2, "Counts not equal");
            for (var i = 0; i < checks.Count; i++)
                if (checks[i] != points[i+1])
                    Assert.Fail();
        }
        #endregion

        internal class SourceData
        {
            internal static IEnumerable ParentChild
            {
                get
                {
                    yield return new TestCaseData("ThreadNew", new object[] { false }, "ThreadNew(System.Boolean)", new List<string>(), "GetStringListForThreadNew(System.Boolean)", new List<string> { "Else_4" });
                    yield return new TestCaseData("ThreadNew", new object[] { true }, "ThreadNew(System.Boolean)", new List<string>(), "GetStringListForThreadNew(System.Boolean)", new List<string> { "If_12" });
                }
            }

            internal static IEnumerable Simple
            {
                get
                {
                    yield return new TestCaseData("IfElse_Half", new object[] { false }, "IfElse_Half(System.Boolean)", new List<string>());
                    yield return new TestCaseData("IfElse_Half", new object[] { true }, "IfElse_Half(System.Boolean)", new List<string> { "If_8" });
                }
            }
        }
    }
}