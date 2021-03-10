using NUnit.Framework;
using Drill4Net.Injector.Core;

namespace Drill4Net.Target.Comon.Tests
{
    public class InjectTargetTests
    {
        private IInjectorRepository _rep;
        private MainOptions _opts;
        //private InjectTarget _target;

        /**************************************************************/

        [OneTimeSetUp]
        public void SetupClass()
        {
            _rep = new InjectorRepository();
            _opts = _rep.GetOptions(null);
        }

        /**************************************************************/

        [Test]
        public void IfElse_Half_False_Ok()
        {
            //act
            //_target.IfElse_Half(false);

            //assert
            //var points = TestProfiler.GetPoints("", "", true);
            Assert.Pass();
        }
    }
}