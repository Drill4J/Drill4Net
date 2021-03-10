using NUnit.Framework;
using Drill4Net.Target.Common;
using Drill4Net.Plugins.Testing;

namespace Drill4Net.Target.Comon.Tests
{
    [TestFixture]
    [SetUpFixture]
    public class InjectTargetTests
    {
        private InjectTarget _target;

        [OneTimeSetUp]
        public void SetupClass()
        {
            _target = new InjectTarget();
        }

        /********************************************/

        [Test]
        public void IfElse_Half_False_Ok()
        {
            //act
            _target.IfElse_Half(false);

            //assert
            TestProfiler.GetPoints("", "", true);
            Assert.Pass();
        }

        [Test]
        public void IfElse_Half_True_Ok()
        {
            Assert.Pass();
        }
    }
}