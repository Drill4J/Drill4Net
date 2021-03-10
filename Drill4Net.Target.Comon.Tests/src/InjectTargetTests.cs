using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Drill4Net.Injector.Core;
using Drill4Net.Injector.Engine;
using Drill4Net.Plugins.Testing;

namespace Drill4Net.Target.Comon.Tests
{
    //Despite the late binding, a reference to the injected target assembly
    //must be added to the project (as file), which in turn must be updated
    //externally each time the injection process is started (automatically)

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
            _rep = new InjectorRepository();
            _opts = _rep.GetOptions(null);
            var injector = new InjectorEngine(_rep);
            injector.Process(Array.Empty<string>());

            var profPath = Path.Combine(_opts.Destination.Directory, _opts.Tests.AssemblyName);
            var asm = Assembly.LoadFrom(profPath);
            _type = asm.GetType($"{_opts.Tests.Namespace}.{_opts.Tests.Class}");
            _target = Activator.CreateInstance(_type);
        }

        /****************************************************************************/

        [Test]
        public void IfElse_Half_False_Ok()
        {
            //arrange
            var mi = GetMethod("IfElse_Half");
            var requestId = GetRequestId();
            var path = GetMethodPath("IfElse_Half(System.Boolean)");
            var checks = new List<string>();

            //act
            mi.Invoke(_target, new object[] { false });
            var points = TestProfiler.GetPoints(requestId, path, true);

            //assert
            CheckEnterReturn(points);
            Check(points, checks);
        }

        [Test]
        public void IfElse_Half_True_Ok()
        {
            //arrange
            var mi = GetMethod("IfElse_Half");
            var requestId = GetRequestId();
            var path = GetMethodPath("IfElse_Half(System.Boolean)");
            var checks = new List<string>() { "If_8" };

            //act
            mi.Invoke(_target, new object[] { true });
            var points = TestProfiler.GetPoints(requestId, path, true);

            //assert
            CheckEnterReturn(points);
            Check(points, checks);
        }

        /****************************************************************************/

        private string GetRequestId()
        {
            return "0";
        }

        private string GetMethodPath(string sig)
        {
            return $"Drill4Net.Target.Common.dll;System.Void Drill4Net.Target.Common.InjectTarget::{sig}";
        }

        private MethodInfo GetMethod(string name)
        {
            return _type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private void CheckEnterReturn(List<string> points)
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
    }
}