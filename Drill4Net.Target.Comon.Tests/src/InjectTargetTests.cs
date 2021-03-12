using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Drill4Net.Injector.Core;
using Drill4Net.Plugins.Testing;

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
        public void Simple_Ok(string shortSig, object[] args, List<string> parentChecks)
        {
            //arrange
            var mi = GetMethod(shortSig);

            //act
            mi.Invoke(_target, args);

            //assert
            var funcs = GetFunctions();
            Assert.IsTrue(funcs.Count == 1);

            var sig = GetSource(shortSig);
            Assert.True(funcs.ContainsKey(sig));
            var points = funcs[sig];

            CheckEnterAndLastReturn(points);
            Check(points, parentChecks);
        }

        [TestCaseSource(typeof(SourceData), "ParentChild", Category = "ParentChild")]
        public void Parent_Child_Ok(object[] args, string parentShortSig, List<string> parentChecks, 
                                                   string childShortSig, List<string> childChecks)
        {
            //arrange
            var mi = GetMethod(parentShortSig);

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
            var ar = shortSig.Split(' ');
            var ret = ar[0];
            var name = ar[1];
            return $"{ret} Drill4Net.Target.Common.InjectTarget::{name}";
        }

        private Dictionary<string, List<string>> GetFunctions()
        {
            return TestProfiler.GetFunctions(false);
        }

        private MethodInfo GetMethod(string shortSig)
        {
            var name = shortSig.Split(' ')[1];
            name = name.Substring(0, name.IndexOf("("));
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
                    yield return new TestCaseData(new object[] { false }, "System.Void ThreadNew(System.Boolean)", new List<string>(), "System.Void GetStringListForThreadNew(System.Boolean)", new List<string> { "Else_4" });
                    yield return new TestCaseData(new object[] { true }, "System.Void ThreadNew(System.Boolean)", new List<string>(), "System.Void GetStringListForThreadNew(System.Boolean)", new List<string> { "If_12" });
                }
            }

            internal static IEnumerable Simple
            {
                get
                {
                    yield return new TestCaseData("System.Void IfElse_Half(System.Boolean)", new object[] { false }, new List<string>());
                    yield return new TestCaseData("System.Void IfElse_Half(System.Boolean)", new object[] { true }, new List<string> { "If_8" });

                    yield return new TestCaseData("System.Void IfElse_FullSimple(System.Boolean)", new object[] { false }, new List<string> { "Else_11" });
                    yield return new TestCaseData("System.Void IfElse_FullSimple(System.Boolean)", new object[] { true }, new List<string> { "If_6" });

                    yield return new TestCaseData("System.Void IfElse_Consec_Full(System.Boolean,System.Boolean)", new object[] { false, false }, new List<string> { "Else_24", "Else_55" });
                    yield return new TestCaseData("System.Void IfElse_Consec_Full(System.Boolean,System.Boolean)", new object[] { false, true },  new List<string> { "Else_24", "If_41" });
                    yield return new TestCaseData("System.Void IfElse_Consec_Full(System.Boolean,System.Boolean)", new object[] { true, false }, new List<string> { "If_10", "Else_55" });
                    yield return new TestCaseData("System.Void IfElse_Consec_Full(System.Boolean,System.Boolean)", new object[] { true, true }, new List<string> { "If_10", "If_41" });

                    yield return new TestCaseData("System.Boolean IfElse_Consec_HalfA_FullB(System.Boolean,System.Boolean)", new object[] { false, false }, new List<string> { "Else_25" });
                    yield return new TestCaseData("System.Boolean IfElse_Consec_HalfA_FullB(System.Boolean,System.Boolean)", new object[] { false, true }, new List<string> { "If_17" });
                    yield return new TestCaseData("System.Boolean IfElse_Consec_HalfA_FullB(System.Boolean,System.Boolean)", new object[] { true, false }, new List<string> { "If_6", "Else_25" });
                    yield return new TestCaseData("System.Boolean IfElse_Consec_HalfA_FullB(System.Boolean,System.Boolean)", new object[] { true, true }, new List<string> { "If_6", "If_17" });

                    yield return new TestCaseData("System.Boolean IfElse_Half_EarlyReturn_Bool(System.Boolean)", new object[] { false }, new List<string>());
                    yield return new TestCaseData("System.Boolean IfElse_Half_EarlyReturn_Bool(System.Boolean)", new object[] { true }, new List<string> { "If_8" });

                    yield return new TestCaseData("System.ValueTuple`2<System.Boolean,System.Boolean> IfElse_Half_EarlyReturn_Tuple(System.Boolean)", new object[] { false }, new List<string>());
                    yield return new TestCaseData("System.ValueTuple`2<System.Boolean,System.Boolean> IfElse_Half_EarlyReturn_Tuple(System.Boolean)", new object[] { true }, new List<string> { "If_8" });
                }
            }
        }
    }
}