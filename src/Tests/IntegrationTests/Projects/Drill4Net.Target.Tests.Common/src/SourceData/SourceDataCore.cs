using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Drill4Net.Target.Common;

namespace Drill4Net.Target.Tests.Common
{
    public static class SourceDataCore
    {
        #region Delegates
        public delegate void EmptySig();
        public delegate void OneBoolMethod(bool cond);
        public delegate void TwoBoolMethod(bool cond, bool cond2);

        public delegate bool OneBool();
        public delegate bool OneBoolFunc(bool cond);
        public delegate bool TwoBoolFunc(bool cond, bool cond2);

#if !NET461
        public delegate (bool, bool) OneBoolTupleFunc(bool cond);
        public delegate IAsyncEnumerable<int> FuncAsyncEnum();
#endif
        public delegate void OneIntMethod(int digit);
        public delegate string OneIntFuncStr(int digit);
        public delegate double OneDoubleFuncDouble(double digit);

        public delegate string OneBoolFuncStr(bool cond);
        public delegate string OneNullBoolFuncStr(bool? cond);
        public delegate string TwoStringFuncStr(string a, string b);
        public delegate Task FuncTask();
        public delegate void OneString(string par);
        public delegate Task OneIntFuncTask(int digit);
        public delegate string FuncString();
        public delegate Task OneBoolFuncTask(bool digit);
        public delegate List<GenStr> FuncListGetStr();
        public delegate Task<GenStr> ProcessElementDlg(GenStr element, bool cond);
        public delegate List<string> OneBoolFuncListStr(bool cond);
        public delegate IEnumerable<string> OneBoolFuncIEnumerable(bool digit);
        public delegate bool OneInt(int digit);
        #endregion
        #region Method info
        public static MethodInfo GetInfo(OneDoubleFuncDouble method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(TwoStringFuncStr method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(OneNullBoolFuncStr method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(OneBool method)
        {
            return method.Method;
        }

#if !NET461
        public static MethodInfo GetInfo(FuncAsyncEnum method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(OneBoolTupleFunc method)
        {
            return method.Method;
        }
#endif
        public static MethodInfo GetInfo(OneString method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(OneIntFuncTask method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(OneBoolFuncIEnumerable method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(EmptySig method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(OneInt method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(OneBoolMethod method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(TwoBoolMethod method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(OneBoolFunc method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(TwoBoolFunc method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(OneIntMethod method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(OneIntFuncStr method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(OneBoolFuncStr method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(OneBoolFuncTask method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(FuncTask method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(FuncListGetStr method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(ProcessElementDlg method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(FuncString method)
        {
            return method.Method;
        }

        public static MethodInfo GetInfo(OneBoolFuncListStr method)
        {
            return method.Method;
        }
        #endregion
        #region GetCase
        //simple
        public static TestCaseData GetCase(MethodInfo mi, object[] pars, List<string> checks)
        {
            var name = mi.Name;
            var caption = GetCaption(name, pars);
            var category = GetCategory(name);
            return new TestCaseData(mi, pars, checks)
                .SetCategory(category)
                .SetName(caption);
        }

        public static TestCaseData GetCase(object[] pars, params TestInfo[] input)
        {
            return GetCase(pars, false, false, false, input);
        }

        public static TestCaseData GetCase(object[] pars, bool ignoreEnterReturns, params TestInfo[] input)
        {
            return GetCase(pars, false, false, ignoreEnterReturns, input);
        }

        public static TestCaseData GetCase(object[] pars, bool isAsync, bool ignoreEnterReturns,
            params TestInfo[] input)
        {
            return GetCase(pars, isAsync, false, ignoreEnterReturns,input);
        }

        //parented
        public static TestCaseData GetCase(object[] pars, bool isAsync, bool isBunch, bool ignoreEnterReturns, params TestInfo[] input)
        {
            Assert.IsNotNull(input);
            Assert.True(input.Length > 0);

            var mi = input[0].Info;
            if (mi == null)
                return null;
            var name = mi.Name;
            var caption = GetCaption(name, pars);
            var category = GetCategory(name);
            return new TestCaseData(mi, pars, isAsync, isBunch, ignoreEnterReturns, input)
                .SetCategory(category)
                .SetName(caption);
        }

        internal static string GetNameFromSignature(string sig)
        {
            if (!sig.Contains(":") || !sig.Contains("."))
                return sig;
            var ar = sig.Split(':')[0].Split('.');
            return ar[ar.Length - 1];
        }

        private static string GetCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            return name.Split('_')[0];
        }

        private static string GetCaption(string name, object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return name;
            name += ": ";
            var lastInd = parameters.Length - 1;
            for (int i = 0; i <= lastInd; i++)
            {
                var par = parameters[i];
                name += par ?? "null";
                if (i < lastInd)
                    name += ",";
            }
            return name;
        }
        #endregion
        #region Source
        public static string GetSource(object target, string shortSig)
        {
            return GetSourceFromFullSig(target, GetFullSignature(target, shortSig));
        }

        public static string GetSourceFromFullSig(Assembly asm, string fullSig)
        {
            var asmName = GetModuleName(asm);
            return CreateMethodSource(asmName, fullSig);
        }

        public static string GetSourceFromFullSig(object target, string fullSig)
        {
            if (fullSig.Contains(";"))
                return fullSig;
            var asmName = GetModuleName(target);
            return CreateMethodSource(asmName, fullSig);
        }

        internal static string CreateMethodSource(string moduleName, string methodFullSig)
        {
            return $"{moduleName};{methodFullSig}";
        }

        internal static string GetFullSignature(object target, string shortSig)
        {
            var asm = GetAssembly(target);
            return GetFullSignatureFromAssembly(asm, shortSig);
        }

        internal static string GetFullSignatureFromAssembly(Assembly asm, string shortSig)
        {
            var ar = shortSig.Split(' ');
            var ret = ar[0];
            var name = ar[1];
            return $"{ret} {asm.GetName().Name}.InjectTarget::{name}";
        }

        public static string GetFullSignature(MethodInfo mi)
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

        internal static string GetModuleName(Assembly asm)
        {
            return asm.ManifestModule.Name;
        }

        internal static string GetModuleName(object target)
        {
            return GetAssembly(target).ManifestModule.Name;
        }

        internal static Assembly GetAssembly(object target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            return target.GetType().Assembly;
        }

        public static string GetNameFromSig(string shortSig)
        {
            var name = shortSig.Split(' ')[1];
            name = name.Substring(0, name.IndexOf("("));
            return name;
        }
        #endregion
    }
}
