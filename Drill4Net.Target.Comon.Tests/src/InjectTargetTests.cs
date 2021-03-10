using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Drill4Net.Injector.Core;
using Drill4Net.Injector.Engine;
using Drill4Net.Plugins.Testing;

namespace Drill4Net.Target.Comon.Tests
{
    public class InjectTargetTests
    {
        private IInjectorRepository _rep;
        private MainOptions _opts;
        private Type _type;
        private object _target;

        /********************************************************************/

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

        /********************************************************************/

        [Test]
        public void IfElse_Half_False_Ok()
        {
            //arrange
            var mi = GetMethod("IfElse_Half");

            //act
            mi.Invoke(_target, new object[] { false });

            //assert
            var points = TestProfiler.GetPoints("", "", true);
            Assert.Pass();
        }

        private MethodInfo GetMethod(string name)
        {
            return _type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
}