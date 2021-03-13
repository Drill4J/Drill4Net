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

        [TestCaseSource(typeof(SourceData), "Simple"/*, Category = "Simple"*/)]
        public void Simple_Ok(string shortSig, object[] args, List<string> checks)
        {
            //arrange
            var mi = GetMethod(shortSig);
            Assert.NotNull(mi, $"MethodInfo is empty for: {shortSig}");

            //act
            mi.Invoke(_target, args);

            //assert
            var funcs = GetFunctions();
            Assert.IsTrue(funcs.Count == 1);

            var sig = GetSource(shortSig);
            Assert.True(funcs.ContainsKey(sig));
            var points = funcs[sig];

            CheckEnterAndLastReturn(points);
            Check(points, checks);
        }

        [TestCaseSource(typeof(SourceData), "ParentChild", Category = "ParentChild")]
        public void Parent_Child_Ok(object[] args, string parentShortSig, List<string> parentChecks, 
                                                   string childShortSig, List<string> childChecks)
        {
            //arrange
            var mi = GetMethod(parentShortSig);
            Assert.NotNull(mi, $"MethodInfo is empty for: {parentShortSig}");

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
            var name = GetNameFromSig(shortSig);
            return _type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private string GetNameFromSig(string shortSig)
        {
            var name = shortSig.Split(' ')[1];
            name = name.Substring(0, name.IndexOf("("));
            return name;
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
            #region Delegates
            internal delegate void OneBoolMethod(bool cond);
            internal delegate void TwoBoolMethod(bool cond, bool cond2);

            internal delegate bool OneBoolFunc(bool cond);
            internal delegate bool TwoBoolFunc(bool cond, bool cond2);

            internal delegate (bool,bool) OneBoolTupleFunc(bool cond);
            #endregion

            private static readonly Common.InjectTarget _target;

            /*****************************************************/

            static SourceData()
            {
                _target = new Common.InjectTarget();
            }

            /******************************************************************/

            internal static IEnumerable ParentChild
            {
                get
                {
                    yield return GetCase(GetInfo(_target.ThreadNew), new object[] { false }, new List<string>(), GetInfo(_target.GetStringListForThreadNew), new List<string> { "Else_4" });
                    yield return GetCase(GetInfo(_target.ThreadNew), new object[] { true }, new List<string>(), GetInfo(_target.GetStringListForThreadNew), new List<string> { "If_12" });
                }
            }

            internal static IEnumerable Simple
            {
                get
                {
                    yield return GetCase(GetInfo(_target.IfElse_Half), new object[] { false }, new List<string>());
                    yield return GetCase(GetInfo(_target.IfElse_Half), new object[] { true }, new List<string> { "If_8" });

                    yield return GetCase(GetInfo(_target.IfElse_FullSimple), new object[] { false }, new List<string> { "Else_11" });
                    yield return GetCase(GetInfo(_target.IfElse_FullSimple), new object[] { true }, new List<string> { "If_6" });

                    yield return GetCase(GetInfo(_target.IfElse_Consec_Full), new object[] { false, false }, new List<string> { "Else_24", "Else_55" });
                    yield return GetCase(GetInfo(_target.IfElse_Consec_Full), new object[] { false, true },  new List<string> { "Else_24", "If_41" });
                    yield return GetCase(GetInfo(_target.IfElse_Consec_Full), new object[] { true, false }, new List<string> { "If_10", "Else_55" });
                    yield return GetCase(GetInfo(_target.IfElse_Consec_Full), new object[] { true, true }, new List<string> { "If_10", "If_41" });

                    yield return GetCase(GetInfo(_target.IfElse_Consec_HalfA_FullB), new object[] { false, false }, new List<string> { "Else_25" });
                    yield return GetCase(GetInfo(_target.IfElse_Consec_HalfA_FullB), new object[] { false, true }, new List<string> { "If_17" });
                    yield return GetCase(GetInfo(_target.IfElse_Consec_HalfA_FullB), new object[] { true, false }, new List<string> { "If_6", "Else_25" });
                    yield return GetCase(GetInfo(_target.IfElse_Consec_HalfA_FullB), new object[] { true, true }, new List<string> { "If_6", "If_17" });

                    yield return GetCase(GetInfo(_target.IfElse_Half_EarlyReturn_Bool), new object[] { false }, new List<string>());
                    yield return GetCase(GetInfo(_target.IfElse_Half_EarlyReturn_Bool), new object[] { true }, new List<string> { "If_8" });

                    yield return GetCase(GetInfo(_target.IfElse_Half_EarlyReturn_Tuple), new object[] { false }, new List<string>());                                                 
                    yield return GetCase(GetInfo(_target.IfElse_Half_EarlyReturn_Tuple), new object[] { true }, new List<string> { "If_8" });

                    yield return GetCase(GetInfo(_target.Ternary_Positive), new object[] { false }, new List<string> { "Else_4" });
                    yield return GetCase(GetInfo(_target.Ternary_Positive), new object[] { true }, new List<string> { "If_8" });

                    yield return GetCase(GetInfo(_target.Ternary_Negative), new object[] { false }, new List<string> { "Else_8" });
                    yield return GetCase(GetInfo(_target.Ternary_Negative), new object[] { true }, new List<string> { "If_4" });

                    yield return GetCase(GetInfo(_target.IfElse_FullCompound), new object[] { false, false }, new List<string> { "Else_30", "Else_45" });
                    yield return GetCase(GetInfo(_target.IfElse_FullCompound), new object[] { false, true }, new List<string> { "Else_30", "If_37" });
                    yield return GetCase(GetInfo(_target.IfElse_FullCompound), new object[] { true, false }, new List<string> { "If_6", "Else_21" });
                    yield return GetCase(GetInfo(_target.IfElse_FullCompound), new object[] { true, true }, new List<string> { "If_6", "If_13" });

                    yield return GetCase(GetInfo(_target.IfElse_HalfA_FullB), new object[] { false, false }, new List<string>());
                    yield return GetCase(GetInfo(_target.IfElse_HalfA_FullB), new object[] { true, false }, new List<string> { "If_6", "Else_21" });
                    yield return GetCase(GetInfo(_target.IfElse_HalfA_FullB), new object[] { true, true }, new List<string> { "If_6", "If_13" });

                    yield return GetCase(GetInfo(_target.IfElse_HalfA_HalfB), new object[] { true, false }, new List<string> { "If_6" });
                    yield return GetCase(GetInfo(_target.IfElse_HalfA_HalfB), new object[] { true, true }, new List<string> { "If_6", "If_13" });

                    yield return GetCase(GetInfo(_target.IfElse_FullA_HalfB), new object[] { false, false }, new List<string> { "Else_22" });
                    yield return GetCase(GetInfo(_target.IfElse_FullA_HalfB), new object[] { true, false }, new List<string> { "If_6" });                    
                    yield return GetCase(GetInfo(_target.IfElse_FullA_HalfB), new object[] { true, true }, new List<string> { "If_6", "If_13" });
                }
            }

            /******************************************************************/

            #region Method info
            internal static MethodInfo GetInfo(OneBoolMethod method)
            {
                return method.Method;
            }

            internal static MethodInfo GetInfo(TwoBoolMethod method)
            {
                return method.Method;
            }

            internal static MethodInfo GetInfo(OneBoolFunc method)
            {
                return method.Method;
            }

            internal static MethodInfo GetInfo(TwoBoolFunc method)
            {
                return method.Method;
            }

            internal static MethodInfo GetInfo(OneBoolTupleFunc method)
            {
                return method.Method;
            }
            #endregion

            internal static TestCaseData GetCase(MethodInfo mi, object[] pars, List<string> checks)
            {
                var name = mi.Name;
                var caption = GetCaption(name, pars);
                var sig = GetFullSignature(mi);
                return new TestCaseData(sig, pars, checks).SetName(caption);
            }

            internal static TestCaseData GetCase(MethodInfo parentMi, object[] pars, List<string> parentChecks,
                                                 MethodInfo childMi, List<string> childChecks)
            {
                var parent = parentMi.Name;
                var caption = GetCaption(parent, pars);
                var parentSig = GetFullSignature(parentMi);
                var childSig = GetFullSignature(childMi);

                return new TestCaseData(pars, parentSig, parentChecks, childSig, childChecks).SetName(caption);
            }

            internal static string GetFullSignature(MethodInfo mi)
            {
                var name = mi.Name;

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
                        .Replace("[","<").Replace("]",">").Replace(" ",null);
                }
                var sig = $"{retType} {name}({parNames})";

                return sig;
            }

            private static string GetCaption(string name, object[] pars)
            {
                name += ": ";
                var lastInd = pars.Length - 1;
                for (int i = 0; i <= lastInd; i++)
                {
                    var par = pars[i];
                    name += par;
                    if (i < lastInd)
                        name += ",";
                }
                return name;
            }
        }
    }
}